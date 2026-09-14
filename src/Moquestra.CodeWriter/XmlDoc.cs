using System;
using System.Collections.Generic;

namespace Moquestra.CodeWriter
{
    /// <summary>
    /// Assembles XML documentation comment lines for generated code. Tags are
    /// rendered in call order, and <see cref="ToString"/> joins the lines with
    /// LF without a comment prefix. Text carries inline markup as-is and is not
    /// escaped; attribute values are escaped.
    /// </summary>
    internal sealed class XmlDoc
    {
        private readonly List<string> _lines = new List<string>();

        /// <summary>Adds a <c>summary</c> tag.</summary>
        /// <param name="text">The text. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        public XmlDoc Summary(string text)
        {
            return Element("summary", string.Empty, text);
        }

        /// <summary>Adds a <c>remarks</c> tag.</summary>
        /// <param name="text">The text. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        public XmlDoc Remarks(string text)
        {
            return Element("remarks", string.Empty, text);
        }

        /// <summary>Adds a <c>returns</c> tag.</summary>
        /// <param name="text">The text. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        public XmlDoc Returns(string text)
        {
            return Element("returns", string.Empty, text);
        }

        /// <summary>Adds a <c>value</c> tag.</summary>
        /// <param name="text">The text. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        public XmlDoc Value(string text)
        {
            return Element("value", string.Empty, text);
        }

        /// <summary>Adds an <c>example</c> tag.</summary>
        /// <param name="text">The text. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        public XmlDoc Example(string text)
        {
            return Element("example", string.Empty, text);
        }

        /// <summary>Adds a <c>param</c> tag.</summary>
        /// <param name="name">The parameter name. Cannot be null or empty.</param>
        /// <param name="text">The text. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="text"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
        public XmlDoc Param(string name, string text)
        {
            return Element("param", Attribute("name", name), text);
        }

        /// <summary>Adds a <c>typeparam</c> tag.</summary>
        /// <param name="name">The type parameter name. Cannot be null or empty.</param>
        /// <param name="text">The text. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="text"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
        public XmlDoc TypeParam(string name, string text)
        {
            return Element("typeparam", Attribute("name", name), text);
        }

        /// <summary>Adds an <c>exception</c> tag.</summary>
        /// <param name="cref">The exception type reference. Cannot be null or empty.</param>
        /// <param name="text">The text. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="cref"/> or <paramref name="text"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="cref"/> is empty.</exception>
        public XmlDoc Exception(string cref, string text)
        {
            return Element("exception", Attribute("cref", cref), text);
        }

        /// <summary>Adds an <c>inheritdoc</c> tag without a reference.</summary>
        public XmlDoc InheritDoc()
        {
            _lines.Add("<inheritdoc/>");

            return this;
        }

        /// <summary>Adds an <c>inheritdoc</c> tag with a reference.</summary>
        /// <param name="cref">The member whose documentation is inherited. Cannot
        /// be null or empty.</param>
        /// <exception cref="ArgumentNullException"><paramref name="cref"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="cref"/> is empty.</exception>
        public XmlDoc InheritDoc(string cref)
        {
            _lines.Add("<inheritdoc" + Attribute("cref", cref) + "/>");

            return this;
        }

        /// <summary>Adds a <c>seealso</c> tag.</summary>
        /// <param name="cref">The member to reference. Cannot be null or empty.</param>
        /// <exception cref="ArgumentNullException"><paramref name="cref"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="cref"/> is empty.</exception>
        public XmlDoc SeeAlso(string cref)
        {
            _lines.Add("<seealso" + Attribute("cref", cref) + "/>");

            return this;
        }

        /// <summary>
        /// Returns the accumulated lines joined with LF, without a comment
        /// prefix.
        /// </summary>
        /// <returns>The comment lines, or an empty string when no tag was added.</returns>
        public override string ToString()
        {
            return string.Join("\n", _lines);
        }

        /// <summary>
        /// Replaces <c>&amp;</c>, <c>&lt;</c>, and <c>&gt;</c> in a value that is
        /// inserted into text verbatim.
        /// </summary>
        /// <param name="value">The value to escape. Cannot be null.</param>
        /// <returns>The escaped value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public static string Escape(string value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        // Adds the element inline when the text is a single line and as a block
        // otherwise.
        private XmlDoc Element(string tag, string attributes, string text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var lines = SplitLines(text);

            if (lines.Count == 1)
            {
                _lines.Add("<" + tag + attributes + ">" + lines[0] + "</" + tag + ">");

                return this;
            }

            _lines.Add("<" + tag + attributes + ">");
            _lines.AddRange(lines);
            _lines.Add("</" + tag + ">");

            return this;
        }

        // The attribute name doubles as the parameter name, so it is also the
        // paramName of the exceptions.
        private static string Attribute(string name, string value)
        {
            if (value is null)
                throw new ArgumentNullException(name);

            if (value.Length == 0)
                throw new ArgumentException("The value cannot be empty.", name);

            return " " + name + "=\"" + EscapeAttribute(value) + "\"";
        }

        // Attribute values are structural, so quotes are escaped as well.
        private static string EscapeAttribute(string value)
        {
            return Escape(value).Replace("\"", "&quot;");
        }

        // Treats CRLF, CR, and LF as line boundaries.
        private static List<string> SplitLines(string text)
        {
            var lines = new List<string>();
            var start = 0;

            for (var i = 0; i < text.Length; i++)
            {
                var current = text[i];

                if (current != '\r' && current != '\n')
                    continue;

                lines.Add(text.Substring(start, i - start));

                if (current == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    i++;

                start = i + 1;
            }

            lines.Add(text.Substring(start));

            return lines;
        }
    }
}
