using System;
using System.Globalization;

using Xunit;

namespace Moquestra.CodeWriter.Tests
{
    public class CodeBuilderTests
    {
        [Fact]
        public void Write_WithNullText_ThrowsArgumentNullException()
        {
            var builder = new CodeBuilder();

            Assert.Throws<ArgumentNullException>(() => builder.Write(null!));
        }

        [Fact]
        public void Write_WithLiteralText_AppendsText()
        {
            var builder = new CodeBuilder();

            builder.Write($"internal sealed class Sample");

            Assert.Equal("internal sealed class Sample", builder.ToString());
        }

        [Fact]
        public void Write_WithSingleLineInterpolatedValue_RendersValue()
        {
            var builder = new CodeBuilder();
            var name = "Sample";

            builder.Write($"class {name} {{ }}");

            Assert.Equal("class Sample { }", builder.ToString());
        }

        [Fact]
        public void Write_WithNullValue_RendersEmpty()
        {
            var builder = new CodeBuilder();
            object value = null!;

            builder.Write($"a{value}b");

            Assert.Equal("ab", builder.ToString());
        }

        [Fact]
        public void Write_CalledMultipleTimes_AppendsInOrder()
        {
            var builder = new CodeBuilder();

            builder.Write($"var a = {1};");
            builder.Write($"var b = {2};");

            Assert.Equal("var a = 1;var b = 2;", builder.ToString());
        }

        [Fact]
        public void Write_WithFormattableValueUnderNonInvariantCulture_RendersInvariant()
        {
            var original = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            try
            {
                var builder = new CodeBuilder();

                builder.Write($"var ratio = {3.5};");

                Assert.Equal("var ratio = 3.5;", builder.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [Fact]
        public void Write_WithMultilineValue_ReindentsToInterpolationColumn()
        {
            var builder = new CodeBuilder();
            var body = "a();\nb();";

            builder.Write($"    {body}");

            Assert.Equal("    a();\n    b();", builder.ToString());
        }

        [Fact]
        public void Write_WithMultilineValueAcrossWriteCalls_AlignsRegardlessOfCallSplit()
        {
            var body = "a();\nb();";
            var single = new CodeBuilder();
            var split = new CodeBuilder();

            single.Write($"void M()\n{{\n    {body}\n}}");
            split.Write($"void M()\n{{\n    ");
            split.Write($"{body}\n}}");

            Assert.Equal("void M()\n{\n    a();\n    b();\n}", single.ToString());
            Assert.Equal(single.ToString(), split.ToString());
        }

        [Fact]
        public void Write_WithMultilineValueContainingEmptyLine_LeavesEmptyLineUnindented()
        {
            var builder = new CodeBuilder();
            var body = "a();\n\nb();";

            builder.Write($"    {body}");

            Assert.Equal("    a();\n\n    b();", builder.ToString());
        }

        [Fact]
        public void Write_WithNestedBuilder_ReindentsRenderedText()
        {
            var inner = new CodeBuilder();
            inner.Write($"a({1});\nb({2});");

            var outer = new CodeBuilder();

            outer.Write($"  {inner}");

            Assert.Equal("  a(1);\n  b(2);", outer.ToString());
        }

        [Fact]
        public void Write_WithEscapedBraces_AppendsLiteralBraces()
        {
            var builder = new CodeBuilder();

            builder.Write($"if (ok) {{ return; }}");

            Assert.Equal("if (ok) { return; }", builder.ToString());
        }

        [Fact]
        public void Write_WithFormatSpecifier_ThrowsFormatException()
        {
            var builder = new CodeBuilder();

            Assert.Throws<FormatException>(() => builder.Write($"{255:X}"));
        }

        [Fact]
        public void Write_WithAlignmentSpecifier_ThrowsFormatException()
        {
            var builder = new CodeBuilder();

            Assert.Throws<FormatException>(() => builder.Write($"{42,-10}"));
        }

        [Fact]
        public void ToString_WithoutWrites_ReturnsEmptyString()
        {
            var builder = new CodeBuilder();

            Assert.Equal(string.Empty, builder.ToString());
        }
    }
}
