using System;
using System.Globalization;
using System.Text;

namespace Moquestra.CodeWriter
{
    /// <summary>
    /// Accumulates text for code generation templates. Literal segments are
    /// written as-is, and multiline interpolated values are reindented from
    /// their second line with a continuation prefix derived from the line
    /// where the interpolation begins.
    /// </summary>
    internal sealed class CodeBuilder
    {
        private readonly StringBuilder _builder = new StringBuilder();

        /// <summary>
        /// Renders the interpolated string with the invariant culture and
        /// accumulates it.
        /// </summary>
        /// <param name="text">The interpolated string to accumulate. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        /// <exception cref="FormatException">The interpolated string contains a
        /// malformed interpolation hole, an unsupported format or alignment
        /// component, an out-of-range argument index, or an unmatched brace.</exception>
        public void Write(FormattableString text)
        {
            Write(text, PreservedPrefixParts.None);
        }

        /// <summary>
        /// Renders the interpolated string with the invariant culture and
        /// accumulates it. On continuation lines of a multiline value, only the
        /// character kinds selected by <paramref name="prefixParts"/> are preserved
        /// in the prefix; the rest are each replaced with an ASCII space.
        /// </summary>
        /// <param name="text">The interpolated string to accumulate. Cannot be null.</param>
        /// <param name="prefixParts">The character kinds preserved verbatim in
        /// continuation line prefixes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="prefixParts"/>
        /// contains undefined flags.</exception>
        /// <exception cref="FormatException">The interpolated string contains a
        /// malformed interpolation hole, an unsupported format or alignment
        /// component, an out-of-range argument index, or an unmatched brace.</exception>
        public void Write(FormattableString text, PreservedPrefixParts prefixParts)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            if ((prefixParts & ~PreservedPrefixParts.All) != 0)
                throw new ArgumentOutOfRangeException(nameof(prefixParts));

            var format = text.Format;

            for (var i = 0; i < format.Length; i++)
            {
                var current = format[i];

                if (current == '{')
                {
                    if (i + 1 < format.Length &&
                        format[i + 1] == '{')
                    {
                        _builder.Append('{');
                        i++;
                    }
                    else
                    {
                        i = AppendArgument(text, format, i, prefixParts);
                    }

                    continue;
                }

                if (current == '}')
                {
                    if (i + 1 < format.Length &&
                        format[i + 1] == '}')
                    {
                        _builder.Append('}');
                        i++;
                    }
                    else
                    {
                        throw new FormatException("An unmatched '}' was found.");
                    }

                    continue;
                }

                _builder.Append(current);
            }
        }

        /// <summary>
        /// Returns all accumulated text.
        /// </summary>
        /// <returns>The accumulated text.</returns>
        public override string ToString()
        {
            return _builder.ToString();
        }

        // Parses an argument index, renders and accumulates the corresponding
        // argument, and returns the closing brace position.
        private int AppendArgument(FormattableString text, string format, int openIndex, PreservedPrefixParts prefixParts)
        {
            var index = 0;
            var digitCount = 0;
            var i = openIndex + 1;

            while (i < format.Length && format[i] >= '0' && format[i] <= '9')
            {
                index = (index * 10) + (format[i] - '0');
                digitCount++;
                i++;
            }

            if (digitCount == 0 || i >= format.Length || format[i] != '}')
                throw new FormatException("The interpolation hole is malformed or contains an unsupported format or alignment component.");

            if (index >= text.ArgumentCount)
                throw new FormatException("The interpolation hole refers to an argument index outside the available range.");

            var argument = text.GetArgument(index);
            var rendered = argument is null
                ? string.Empty
                : RenderArgument(argument);

            AppendValue(rendered, prefixParts);

            return i;
        }

        // Reindents a multiline value from its second line with the
        // continuation prefix. Empties whitespace-only continuation lines.
        private void AppendValue(string value, PreservedPrefixParts prefixParts)
        {
            if (value.IndexOf('\n') < 0)
            {
                _builder.Append(value);

                return;
            }

            var prefix = CreateContinuationPrefix(prefixParts);
            var start = 0;

            while (true)
            {
                var newline = value.IndexOf('\n', start);

                if (newline < 0)
                {
                    _builder.Append(value, start, value.Length - start);

                    return;
                }

                _builder.Append(value, start, newline - start);
                _builder.Append('\n');

                start = newline + 1;

                if (start >= value.Length)
                    return;

                if (value[start] == '\n')
                    continue;

                var lineEnd = value.IndexOf('\n', start);

                if (lineEnd >= 0 && IsWhitespaceOnly(value, start, lineEnd))
                {
                    start = lineEnd;

                    continue;
                }

                if (prefix.Length > 0)
                    _builder.Append(prefix);
            }
        }

        // Returns the current line prefix after the last newline, keeping
        // preserved characters verbatim and masking the rest with spaces.
        private string CreateContinuationPrefix(PreservedPrefixParts prefixParts)
        {
            var lineStart = 0;

            for (var i = _builder.Length - 1; i >= 0; i--)
            {
                if (_builder[i] == '\n')
                {
                    lineStart = i + 1;

                    break;
                }
            }

            var length = _builder.Length - lineStart;

            if (length == 0)
                return string.Empty;

            var characters = new char[length];

            for (var i = 0; i < length; i++)
            {
                var current = _builder[lineStart + i];

                characters[i] = IsPreserved(current, prefixParts)
                    ? current
                    : ' ';
            }

            return new string(characters);
        }

        private static bool IsPreserved(char character, PreservedPrefixParts prefixParts)
        {
            if (character == ' ')
                return true;

            if (character == '\t')
                return (prefixParts & PreservedPrefixParts.Tabs) != 0;

            return (prefixParts & PreservedPrefixParts.NonWhitespace) != 0;
        }

        // Treats only spaces and tabs as whitespace.
        private static bool IsWhitespaceOnly(string value, int start, int end)
        {
            for (var i = start; i < end; i++)
            {
                if (value[i] != ' ' && value[i] != '\t')
                    return false;
            }

            return true;
        }

        private static string RenderArgument(object value)
        {
            if (value is null)
                return string.Empty;

            if (value is IFormattable formattable)
                return formattable.ToString(null, CultureInfo.InvariantCulture);

            return value.ToString() ?? string.Empty;
        }
    }
}
