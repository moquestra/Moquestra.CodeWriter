using System;
using System.Globalization;
using System.Text;

namespace Moquestra.CodeWriter
{
    /// <summary>
    /// Accumulates text for code generation templates. Literal segments are
    /// written as-is, and multiline interpolated values are reindented from
    /// their second line to the column where the interpolation begins.
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
            if (text is null)
                throw new ArgumentNullException(nameof(text));

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
                        i = AppendArgument(text, format, i);
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
        private int AppendArgument(FormattableString text, string format, int openIndex)
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
            var rendered = RenderArgument(argument);

            AppendValue(rendered);

            return i;
        }

        // Reindents a multiline value from its second line with spaces matching
        // the column where the interpolation began.
        private void AppendValue(string value)
        {
            var column = CurrentColumn();
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

                if (column > 0)
                    _builder.Append(' ', column);
            }
        }

        private int CurrentColumn()
        {
            for (var i = _builder.Length - 1; i >= 0; i--)
            {
                if (_builder[i] == '\n')
                    return _builder.Length - i - 1;
            }

            return _builder.Length;
        }

        private static string RenderArgument(object value)
        {
            if (value is null)
                return string.Empty;

            if (value is IFormattable formattable)
                return formattable.ToString(null, CultureInfo.InvariantCulture);

            return value.ToString();
        }
    }
}
