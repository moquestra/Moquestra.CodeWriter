using System;

namespace Moquestra.CodeWriter
{
    /// <summary>
    /// Specifies immutable output options for CodeBuilder. By default,
    /// newlines are written as LF, whitespace-only lines are trimmed,
    /// and continuation prefixes are preserved in full.
    /// </summary>
    internal sealed class CodeBuilderSettings
    {
        /// <summary>
        /// The shared default settings instance.
        /// </summary>
        public static CodeBuilderSettings Default { get; } = new CodeBuilderSettings();

        /// <summary>
        /// Creates settings with the specified options.
        /// </summary>
        /// <param name="newLine">The output newline string.</param>
        /// <param name="trimWhitespaceOnlyLines">Whether completed lines
        /// holding only spaces and tabs are emptied.</param>
        /// <param name="prefixParts">The character kinds preserved verbatim in
        /// continuation line prefixes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="newLine"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="newLine"/> is
        /// neither "\n" nor "\r\n".</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="prefixParts"/>
        /// contains undefined flags.</exception>
        public CodeBuilderSettings(
            string newLine = "\n",
            bool trimWhitespaceOnlyLines = true,
            PreservedPrefixParts prefixParts = PreservedPrefixParts.All)
        {
            if (newLine is null)
                throw new ArgumentNullException(nameof(newLine));

            if (newLine != "\n" && newLine != "\r\n")
                throw new ArgumentException("Only \"\\n\" or \"\\r\\n\" is supported.", nameof(newLine));

            if ((prefixParts & ~PreservedPrefixParts.All) != 0)
                throw new ArgumentOutOfRangeException(nameof(prefixParts));

            NewLine = newLine;
            TrimWhitespaceOnlyLines = trimWhitespaceOnlyLines;
            PrefixParts = prefixParts;
        }

        /// <summary>
        /// The output newline string.
        /// </summary>
        public string NewLine { get; }

        /// <summary>
        /// Whether completed lines are emptied when their content other than
        /// generated prefixes consists only of spaces and tabs. Inside a multiline
        /// value, an emptied line keeps its continuation prefix without trailing
        /// whitespace.
        /// </summary>
        public bool TrimWhitespaceOnlyLines { get; }

        /// <summary>
        /// The character kinds preserved verbatim in continuation line
        /// prefixes.
        /// </summary>
        public PreservedPrefixParts PrefixParts { get; }
    }
}
