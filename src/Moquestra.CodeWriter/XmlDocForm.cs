namespace Moquestra.CodeWriter
{
    /// <summary>
    /// Specifies how <see cref="XmlDoc"/> lays out a tag around its text.
    /// </summary>
    internal enum XmlDocForm
    {
        /// <summary>
        /// Renders single-line text inline and multiline text in block form.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Renders the tag inline. Multiline text keeps the opening tag on the
        /// first line and the closing tag at the end of the last line.
        /// </summary>
        Inline = 1,

        /// <summary>
        /// Renders the opening tag, the text lines, and the closing tag on
        /// separate lines.
        /// </summary>
        Block = 2,
    }
}
