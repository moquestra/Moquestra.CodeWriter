using System;

using Xunit;

namespace Moquestra.CodeWriter.Tests
{
    public class XmlDocTests
    {
        [Fact]
        public void Summary_WithSingleLine_RendersInline()
        {
            var doc = new XmlDoc().Summary("Gets the value.");

            Assert.Equal("<summary>Gets the value.</summary>", doc.ToString());
        }

        [Fact]
        public void Summary_WithMultipleLines_RendersBlock()
        {
            var doc = new XmlDoc().Summary("First line.\nSecond line.");

            Assert.Equal("<summary>\nFirst line.\nSecond line.\n</summary>", doc.ToString());
        }

        [Fact]
        public void Summary_WithCarriageReturnLineFeed_SplitsLines()
        {
            var doc = new XmlDoc().Summary("First line.\r\nSecond line.");

            Assert.Equal("<summary>\nFirst line.\nSecond line.\n</summary>", doc.ToString());
        }

        [Fact]
        public void Summary_WithCarriageReturn_SplitsLines()
        {
            var doc = new XmlDoc().Summary("First line.\rSecond line.");

            Assert.Equal("<summary>\nFirst line.\nSecond line.\n</summary>", doc.ToString());
        }

        [Fact]
        public void Summary_WithEmptyText_RendersEmptyInline()
        {
            var doc = new XmlDoc().Summary(string.Empty);

            Assert.Equal("<summary></summary>", doc.ToString());
        }

        [Fact]
        public void Remarks_RendersTag()
        {
            Assert.Equal("<remarks>Text.</remarks>", new XmlDoc().Remarks("Text.").ToString());
        }

        [Fact]
        public void Returns_RendersTag()
        {
            Assert.Equal("<returns>Text.</returns>", new XmlDoc().Returns("Text.").ToString());
        }

        [Fact]
        public void Value_RendersTag()
        {
            Assert.Equal("<value>Text.</value>", new XmlDoc().Value("Text.").ToString());
        }

        [Fact]
        public void Example_RendersTag()
        {
            Assert.Equal("<example>Text.</example>", new XmlDoc().Example("Text.").ToString());
        }

        [Fact]
        public void Param_RendersNameAttribute()
        {
            var doc = new XmlDoc().Param("id", "The ID to look up.");

            Assert.Equal("<param name=\"id\">The ID to look up.</param>", doc.ToString());
        }

        [Fact]
        public void Param_WithMultipleLines_RendersBlockWithAttribute()
        {
            var doc = new XmlDoc().Param("id", "The ID.\nCannot be negative.");

            Assert.Equal("<param name=\"id\">\nThe ID.\nCannot be negative.\n</param>", doc.ToString());
        }

        [Fact]
        public void TypeParam_RendersNameAttribute()
        {
            var doc = new XmlDoc().TypeParam("T", "The element type.");

            Assert.Equal("<typeparam name=\"T\">The element type.</typeparam>", doc.ToString());
        }

        [Fact]
        public void Exception_RendersCrefAttribute()
        {
            var doc = new XmlDoc().Exception("System.ArgumentNullException", "<paramref name=\"type\"/> is null.");

            Assert.Equal(
                "<exception cref=\"System.ArgumentNullException\"><paramref name=\"type\"/> is null.</exception>",
                doc.ToString());
        }

        [Fact]
        public void InheritDoc_WithoutCref_RendersSelfClosingTag()
        {
            Assert.Equal("<inheritdoc/>", new XmlDoc().InheritDoc().ToString());
        }

        [Fact]
        public void InheritDoc_WithCref_RendersCrefAttribute()
        {
            var doc = new XmlDoc().InheritDoc("System.Object.ToString");

            Assert.Equal("<inheritdoc cref=\"System.Object.ToString\"/>", doc.ToString());
        }

        [Fact]
        public void SeeAlso_RendersSelfClosingTag()
        {
            Assert.Equal("<seealso cref=\"System.String\"/>", new XmlDoc().SeeAlso("System.String").ToString());
        }

        [Fact]
        public void SeeAlso_WithGenericCref_EscapesAngleBrackets()
        {
            var doc = new XmlDoc().SeeAlso("System.Collections.Generic.List<T>");

            Assert.Equal("<seealso cref=\"System.Collections.Generic.List&lt;T&gt;\"/>", doc.ToString());
        }

        [Fact]
        public void Param_WithQuoteInName_EscapesQuote()
        {
            var doc = new XmlDoc().Param("a\"b", "Text.");

            Assert.Equal("<param name=\"a&quot;b\">Text.</param>", doc.ToString());
        }

        [Fact]
        public void Exception_WithAmpersandInCref_EscapesAmpersand()
        {
            var doc = new XmlDoc().Exception("A&B", "Text.");

            Assert.Equal("<exception cref=\"A&amp;B\">Text.</exception>", doc.ToString());
        }

        [Fact]
        public void Chain_RendersTagsInCallOrder()
        {
            var doc = new XmlDoc()
                .Returns("The result.")
                .Summary("Computes.")
                .Param("x", "The input.");

            Assert.Equal(
                "<returns>The result.</returns>\n<summary>Computes.</summary>\n<param name=\"x\">The input.</param>",
                doc.ToString());
        }

        [Fact]
        public void ToString_WithoutTags_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, new XmlDoc().ToString());
        }

        [Fact]
        public void Escape_ReplacesReservedCharacters()
        {
            Assert.Equal("a &lt; b &amp;&amp; c &gt; d", XmlDoc.Escape("a < b && c > d"));
        }

        [Fact]
        public void Escape_WithNullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => XmlDoc.Escape(null!));
        }

        [Fact]
        public void Write_WithXmlDocValue_ReplicatesCommentPrefix()
        {
            var doc = new XmlDoc()
                .Summary("First line.\nSecond line.")
                .Param("id", "The ID.");
            var builder = new CodeBuilder();

            builder.Write($"    /// {doc}");

            Assert.Equal(
                "    /// <summary>\n    /// First line.\n    /// Second line.\n    /// </summary>\n    /// <param name=\"id\">The ID.</param>",
                builder.ToString());
        }

        [Fact]
        public void Summary_WithNullText_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new XmlDoc().Summary(null!));
        }

        [Fact]
        public void Param_WithNullName_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new XmlDoc().Param(null!, "Text."));

            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public void Param_WithEmptyName_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() => new XmlDoc().Param(string.Empty, "Text."));

            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public void Exception_WithNullCref_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new XmlDoc().Exception(null!, "Text."));

            Assert.Equal("cref", exception.ParamName);
        }

        [Fact]
        public void SeeAlso_WithEmptyCref_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new XmlDoc().SeeAlso(string.Empty));
        }
    }
}
