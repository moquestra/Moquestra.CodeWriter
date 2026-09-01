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
    /// CRLF, CR, and LF are normalized to the configured output newline.
    /// </summary>
    internal sealed class CodeBuilder
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private readonly CodeBuilderSettings _settings;

        // Index in _builder where the current line starts.
        private int _lineStart;

        // Eligible only while the line contains generated prefixes and value
        // spaces or tabs.
        private bool _lineTrimEligible = true;

        /// <summary>
        /// Creates a builder with the default settings.
        /// </summary>
        public CodeBuilder()
            : this(CodeBuilderSettings.Default)
        {
        }

        /// <summary>
        /// Creates a builder that preserves the character kinds selected by
        /// <paramref name="prefixParts"/> in continuation line prefixes by
        /// default.
        /// </summary>
        /// <param name="prefixParts">The character kinds to preserve verbatim
        /// in continuation line prefixes by default.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="prefixParts"/>
        /// contains undefined flags.</exception>
        public CodeBuilder(PreservedPrefixParts prefixParts)
            : this(new CodeBuilderSettings(prefixParts: prefixParts))
        {
        }

        /// <summary>
        /// Creates a builder with the specified settings.
        /// </summary>
        /// <param name="settings">The output options. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settings"/> is null.</exception>
        public CodeBuilder(CodeBuilderSettings settings)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            _settings = settings;
        }

        /// <summary>
        /// Renders the interpolated string with the invariant culture and
        /// accumulates it. Continuation prefixes of multiline values follow the
        /// preserved parts specified at construction.
        /// </summary>
        /// <param name="text">The interpolated string to accumulate. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        /// <exception cref="FormatException">The interpolated string contains a
        /// malformed interpolation hole, an unsupported format or alignment
        /// component, an out-of-range argument index, or an unmatched brace.</exception>
        public void Write(FormattableString text)
        {
            Write(text, _settings.PrefixParts);
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
                        AppendLiteral('{');
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
                        AppendLiteral('}');
                        i++;
                    }
                    else
                    {
                        throw new FormatException("An unmatched '}' was found.");
                    }

                    continue;
                }

                if (ScanNewLine(format, ref i))
                    continue;

                AppendLiteral(current);
            }
        }

        /// <summary>
        /// Ends the current line with a newline.
        /// </summary>
        public void WriteLine()
        {
            CompleteLine();
        }

        /// <summary>
        /// Renders the interpolated string with the invariant culture and
        /// accumulates it, then ends the current line with a newline.
        /// </summary>
        /// <param name="text">The interpolated string to accumulate. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        /// <exception cref="FormatException">The interpolated string contains a
        /// malformed interpolation hole, an unsupported format or alignment
        /// component, an out-of-range argument index, or an unmatched brace.</exception>
        public void WriteLine(FormattableString text)
        {
            Write(text);
            CompleteLine();
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

        private void AppendValue(string value, PreservedPrefixParts prefixParts)
        {
            if (value.IndexOf('\r') < 0 &&
                value.IndexOf('\n') < 0)
            {
                if (value.Length == 0)
                    return;

                if (!IsWhitespaceOnly(value, 0, value.Length))
                    _lineTrimEligible = false;

                _builder.Append(value);

                return;
            }

            var prefix = CreateContinuationPrefix(prefixParts);
            var needPrefix = false;

            for (var i = 0; i < value.Length; i++)
            {
                if (ScanNewLine(value, ref i))
                {
                    needPrefix = true;

                    continue;
                }

                var current = value[i];

                if (needPrefix)
                {
                    // Generated prefixes are excluded from trim eligibility.
                    if (prefix.Length > 0)
                        _builder.Append(prefix);

                    needPrefix = false;
                }

                if (current != ' ' && current != '\t')
                    _lineTrimEligible = false;

                _builder.Append(current);
            }
        }

        // Returns the current line prefix, keeping preserved characters
        // verbatim and masking the rest with spaces.
        private string CreateContinuationPrefix(PreservedPrefixParts prefixParts)
        {
            var length = _builder.Length - _lineStart;

            if (length == 0)
                return string.Empty;

            var characters = new char[length];

            for (var i = 0; i < length; i++)
            {
                var current = _builder[_lineStart + i];

                characters[i] = IsPreserved(current, prefixParts)
                    ? current
                    : ' ';
            }

            return new string(characters);
        }

        private void AppendLiteral(char character)
        {
            _lineTrimEligible = false;
            _builder.Append(character);
        }

        private bool ScanNewLine(string source, ref int index)
        {
            var current = source[index];

            if (current == '\r')
            {
                if (index + 1 < source.Length && source[index + 1] == '\n')
                    index++;

                CompleteLine();

                return true;
            }

            if (current == '\n')
            {
                CompleteLine();

                return true;
            }

            return false;
        }

        private void CompleteLine()
        {
            if (_lineTrimEligible && _settings.TrimWhitespaceOnlyValueLines)
                _builder.Length = _lineStart;

            AppendNewLine();

            _lineTrimEligible = true;
        }

        private void AppendNewLine()
        {
            _builder.Append(_settings.NewLine);

            _lineStart = _builder.Length;
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
