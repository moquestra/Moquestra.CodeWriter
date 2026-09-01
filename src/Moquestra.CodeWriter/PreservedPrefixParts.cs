using System;

namespace Moquestra.CodeWriter
{
    /// <summary>
    /// Specifies which character kinds of the current line prefix are preserved
    /// verbatim on continuation lines of a multiline interpolated value.
    /// Characters that are not preserved are each replaced with an ASCII
    /// space.
    /// </summary>
    [Flags]
    internal enum PreservedPrefixParts
    {
        /// <summary>
        /// Preserves no characters. Each prefix character is replaced with an
        /// ASCII space.
        /// </summary>
        None = 0,

        /// <summary>
        /// Preserves tab characters.
        /// </summary>
        Tabs = 1 << 0,

        /// <summary>
        /// Preserves characters other than spaces and tabs.
        /// </summary>
        NonWhitespace = 1 << 1,

        /// <summary>
        /// Preserves the whole prefix.
        /// </summary>
        All = Tabs | NonWhitespace,
    }
}
