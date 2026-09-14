using System;

namespace Moquestra.CodeWriter
{
    /// <summary>
    /// Specifies immutable output options for CodeBuilder. By default,
    /// newlines are written as LF, whitespace-only value lines are trimmed,
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
        /// <param name="trimWhitespaceOnlyValueLines">Whether completed lines
        /// holding only value spaces and tabs are emptied.</param>
        /// <param name="prefixParts">The character kinds preserved verbatim in
        /// continuation line prefixes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="newLine"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="newLine"/> is
        /// neither "\n" nor "\r\n".</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="prefixParts"/>
        /// contains undefined flags.</exception>
        public CodeBuilderSettings(
            string newLine = "\n",
            bool trimWhitespaceOnlyValueLines = true,
            PreservedPrefixParts prefixParts = PreservedPrefixParts.All)
        {
            if (newLine is null)
                throw new ArgumentNullException(nameof(newLine));

            if (newLine != "\n" && newLine != "\r\n")
                throw new ArgumentException("Only \"\\n\" or \"\\r\\n\" is supported.", nameof(newLine));

            if ((prefixParts & ~PreservedPrefixParts.All) != 0)
                throw new ArgumentOutOfRangeException(nameof(prefixParts));

            NewLine = newLine;
            TrimWhitespaceOnlyValueLines = trimWhitespaceOnlyValueLines;
            PrefixParts = prefixParts;
        }

        /// <summary>
        /// The output newline string.
        /// </summary>
        public string NewLine { get; }

        /// <summary>
        /// Whether completed lines holding only value spaces and tabs are
        /// emptied. Inside a multiline value, an emptied line keeps its
        /// continuation prefix without trailing whitespace.
        /// </summary>
        public bool TrimWhitespaceOnlyValueLines { get; }

        /// <summary>
        /// The character kinds preserved verbatim in continuation line
        /// prefixes.
        /// </summary>
        public PreservedPrefixParts PrefixParts { get; }
    }
}
