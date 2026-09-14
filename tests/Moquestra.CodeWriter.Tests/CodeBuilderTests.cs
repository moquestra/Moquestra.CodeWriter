using System;
using System.Globalization;
using System.Runtime.CompilerServices;

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
        public void Write_WithMultilineValueContainingConsecutiveEmptyLines_LeavesEmptyLinesUnindented()
        {
            var builder = new CodeBuilder();
            var body = "a();\n\n\nb();";

            builder.Write($"    {body}");

            Assert.Equal("    a();\n\n\n    b();", builder.ToString());
        }

        [Fact]
        public void Write_WithMultilineValueContainingWhitespaceOnlyLine_TrimsLineToEmpty()
        {
            var builder = new CodeBuilder();
            var body = "a();\n \t \nb();";

            builder.Write($"    {body}");

            Assert.Equal("    a();\n\n    b();", builder.ToString());
        }

        [Fact]
        public void Write_WithConsecutiveWhitespaceOnlyLines_TrimsEachLineToEmpty()
        {
            var builder = new CodeBuilder();
            var body = "a();\n \n\t\nb();";

            builder.Write($"    {body}");

            Assert.Equal("    a();\n\n\n    b();", builder.ToString());
        }

        [Fact]
        public void Write_WithUnterminatedWhitespaceOnlyLine_KeepsLineIndented()
        {
            var builder = new CodeBuilder();
            var body = "a();\n   ";

            builder.Write($"    {body}");

            Assert.Equal("    a();\n       ", builder.ToString());
        }

        [Fact]
        public void Write_WithLiteralWhitespaceBeforeWhitespaceOnlyFirstValueLine_TrimsLine()
        {
            var builder = new CodeBuilder();
            var body = "  \nx();";

            builder.Write($"    {body}");

            Assert.Equal("\n    x();", builder.ToString());
        }

        [Fact]
        public void Write_WithTabBeforeInterpolation_PreservesTabOnContinuationLine()
        {
            var builder = new CodeBuilder();
            var body = "a();\nb();";

            builder.Write($"\t{body}");

            Assert.Equal("\ta();\n\tb();", builder.ToString());
        }

        [Fact]
        public void Write_WithIndentedMultilineValue_PreservesValueIndentation()
        {
            var builder = new CodeBuilder();
            var body = "if (x)\n    y();";

            builder.Write($"    {body}");

            Assert.Equal("    if (x)\n        y();", builder.ToString());
        }

        [Fact]
        public void Write_WithMultipleInterpolationsOnOneLine_ReplicatesPrefixIncludingEarlierValues()
        {
            var builder = new CodeBuilder();
            var name = "v";
            var body = "1 +\n2";

            builder.Write($"{name} = {body};");

            Assert.Equal("v = 1 +\nv = 2;", builder.ToString());
        }

        [Fact]
        public void Write_WithLeadingNewlineInValue_ReplicatesPrefixOnNextLine()
        {
            var builder = new CodeBuilder();
            var body = "\n1;";

            builder.Write($"x ={body}");

            Assert.Equal("x =\nx =1;", builder.ToString());
        }

        [Fact]
        public void Write_WithTrailingNewlineInValue_EndsWithoutIndentation()
        {
            var builder = new CodeBuilder();
            var body = "a();\n";

            builder.Write($"    {body}");

            Assert.Equal("    a();\n", builder.ToString());
        }

        [Fact]
        public void Write_WithAllParts_ReplicatesLinePrefix()
        {
            var builder = new CodeBuilder();
            var body = "a\nb";

            builder.Write($"// {body}", PreservedPrefixParts.All);

            Assert.Equal("// a\n// b", builder.ToString());
        }

        [Fact]
        public void Write_WithTabsParts_PreservesTabsAndMasksOthers()
        {
            var builder = new CodeBuilder();
            var body = "a\nb";

            builder.Write($"\t- {body}", PreservedPrefixParts.Tabs);

            Assert.Equal("\t- a\n\t  b", builder.ToString());
        }

        [Fact]
        public void Write_WithNonWhitespaceParts_MasksTabsToSpaces()
        {
            var builder = new CodeBuilder();
            var body = "a\nb";

            builder.Write($"\t- {body}", PreservedPrefixParts.NonWhitespace);

            Assert.Equal("\t- a\n - b", builder.ToString());
        }

        [Fact]
        public void Write_WithNoneParts_MasksWholePrefix()
        {
            var builder = new CodeBuilder();
            var body = "a\nb";

            builder.Write($"\t- {body}", PreservedPrefixParts.None);

            Assert.Equal("\t- a\n   b", builder.ToString());
        }

        [Fact]
        public void Write_WithAllParts_KeepsNonWhitespacePrefixOnEmptyLine()
        {
            var builder = new CodeBuilder();
            var body = "a\n\nb";

            builder.Write($"// {body}", PreservedPrefixParts.All);

            Assert.Equal("// a\n//\n// b", builder.ToString());
        }

        [Fact]
        public void Write_WithAllParts_KeepsNonWhitespacePrefixOnWhitespaceOnlyLine()
        {
            var builder = new CodeBuilder();
            var body = "a\n \nb";

            builder.Write($"// {body}", PreservedPrefixParts.All);

            Assert.Equal("// a\n//\n// b", builder.ToString());
        }

        [Fact]
        public void Write_WithNoneParts_LeavesEmptyLineUnprefixed()
        {
            var builder = new CodeBuilder();
            var body = "a\n\nb";

            builder.Write($"// {body}", PreservedPrefixParts.None);

            Assert.Equal("// a\n\n   b", builder.ToString());
        }

        [Fact]
        public void Write_WithTabPrefixAndEmptyLine_KeepsPrefixWithoutTrailingSpace()
        {
            var builder = new CodeBuilder();
            var body = "a\n\nb";

            builder.Write($"\t- {body}", PreservedPrefixParts.All);

            Assert.Equal("\t- a\n\t-\n\t- b", builder.ToString());
        }

        [Fact]
        public void Write_WithTrimDisabledAndWhitespaceOnlyLine_KeepsFullPrefixAndSpaces()
        {
            var builder = new CodeBuilder(
                new CodeBuilderSettings(trimWhitespaceOnlyLines: false));
            var body = "a\n \nb";

            builder.Write($"// {body}");

            Assert.Equal("// a\n//  \n// b", builder.ToString());
        }

        [Fact]
        public void Write_WithTrailingValueNewlineBeforeLiteralNewline_LeavesLineEmpty()
        {
            var builder = new CodeBuilder();
            var body = "a\n";

            builder.Write($"// {body}\nx");

            Assert.Equal("// a\n\nx", builder.ToString());
        }

        [Fact]
        public void Write_WithBuilderDefaultAllParts_ReplicatesLinePrefix()
        {
            var builder = new CodeBuilder(PreservedPrefixParts.All);
            var body = "a\nb";

            builder.Write($"// {body}");

            Assert.Equal("// a\n// b", builder.ToString());
        }

        [Fact]
        public void Write_WithExplicitParts_OverridesBuilderDefault()
        {
            var builder = new CodeBuilder(PreservedPrefixParts.All);
            var body = "a\nb";

            builder.Write($"\t- {body}", PreservedPrefixParts.None);

            Assert.Equal("\t- a\n   b", builder.ToString());
        }

        [Fact]
        public void Constructor_WithUndefinedParts_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new CodeBuilder((PreservedPrefixParts)4));
        }

        [Fact]
        public void Write_WithUndefinedParts_ThrowsArgumentOutOfRangeException()
        {
            var builder = new CodeBuilder();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => builder.Write($"a", (PreservedPrefixParts)4));
        }

        [Fact]
        public void Write_WithWhitespaceOnlyLineCompletedAnywhere_MatchesSingleValue()
        {
            var head = "x();\n   ";
            var singleValue = new CodeBuilder();
            var literalSplit = new CodeBuilder();
            var writeSplit = new CodeBuilder();
            var body = "x();\n   \n";

            singleValue.Write($"    {body}");
            literalSplit.Write($"    {head}\n");
            writeSplit.Write($"    {head}");
            writeSplit.Write($"\n");

            Assert.Equal("    x();\n\n", singleValue.ToString());
            Assert.Equal(singleValue.ToString(), literalSplit.ToString());
            Assert.Equal(singleValue.ToString(), writeSplit.ToString());
        }

        [Fact]
        public void Write_WithLiteralSpaceAfterValueWhitespace_TrimsLine()
        {
            var builder = new CodeBuilder();
            var body = "a\n ";

            builder.Write($"{body} \nb");

            Assert.Equal("a\n\nb", builder.ToString());
        }

        [Fact]
        public void Write_WithLiteralWhitespaceOnlyLine_TrimsLine()
        {
            var builder = new CodeBuilder();

            builder.Write($"a\n   \nb");

            Assert.Equal("a\n\nb", builder.ToString());
        }

        [Fact]
        public void Write_WithLiteralTabOnlyLine_TrimsLine()
        {
            var builder = new CodeBuilder();

            builder.Write($"a\n\t\nb");

            Assert.Equal("a\n\nb", builder.ToString());
        }

        [Fact]
        public void Write_WithTrimDisabledAndLiteralWhitespaceOnlyLine_KeepsLine()
        {
            var builder = new CodeBuilder(
                new CodeBuilderSettings(trimWhitespaceOnlyLines: false));

            builder.Write($"a\n   \nb");

            Assert.Equal("a\n   \nb", builder.ToString());
        }

        [Fact]
        public void Write_WithIndentedHoleAndEmptyValue_TrimsIndentation()
        {
            var builder = new CodeBuilder();
            var body = string.Empty;

            builder.Write($"{{\n    {body}\n}}");

            Assert.Equal("{\n\n}", builder.ToString());
        }

        [Fact]
        public void Write_WithPrefixedWhitespaceLineCompletedByNextWrite_TrimsPrefixToo()
        {
            var builder = new CodeBuilder();
            var body = "a\n   ";

            builder.Write($"// {body}");
            builder.Write($"\nb");

            Assert.Equal("// a\n\nb", builder.ToString());
        }

        [Fact]
        public void Write_WithWhitespaceOnlyFirstValueLineWithoutLiteralCharacters_TrimsLine()
        {
            var builder = new CodeBuilder();
            var body = " \na";

            builder.Write($"{body}");

            Assert.Equal("\na", builder.ToString());
        }

        [Fact]
        public void Write_WithCarriageReturnNewlines_NormalizesToLineFeed()
        {
            var crlf = new CodeBuilder();
            var cr = new CodeBuilder();

            crlf.Write($"a\r\nb");
            cr.Write($"a\rb");

            Assert.Equal("a\nb", crlf.ToString());
            Assert.Equal("a\nb", cr.ToString());
        }

        [Fact]
        public void Write_WithCarriageReturnInValue_NormalizesToLineFeed()
        {
            var builder = new CodeBuilder();
            var body = "a\r\nb";

            builder.Write($"    {body}");

            Assert.Equal("    a\n    b", builder.ToString());
        }

        [Fact]
        public void Write_WithCarriageReturnLineFeedSplitAcrossWrites_EmitsTwoNewlines()
        {
            var builder = new CodeBuilder();

            builder.Write($"a\r");
            builder.Write($"\nb");

            Assert.Equal("a\n\nb", builder.ToString());
        }

        [Fact]
        public void Write_WithTrailingCarriageReturn_NormalizesImmediately()
        {
            var builder = new CodeBuilder();

            builder.Write($"a\r");

            Assert.Equal("a\n", builder.ToString());
        }

        [Fact]
        public void Write_WithDefaultSettings_MatchesDefaultBuilder()
        {
            var body = "a\nb";
            var withSettings = new CodeBuilder(new CodeBuilderSettings());
            var withDefaults = new CodeBuilder();

            withSettings.Write($"// {body}");
            withDefaults.Write($"// {body}");

            Assert.Equal(withDefaults.ToString(), withSettings.ToString());
        }

        [Fact]
        public void Write_WithCrlfNewLineSetting_EmitsCrlfForLiteralAndValueNewlines()
        {
            var builder = new CodeBuilder(new CodeBuilderSettings(newLine: "\r\n"));
            var body = "b\nc";

            builder.Write($"a\n{body}");

            Assert.Equal("a\r\nb\r\nc", builder.ToString());
        }

        [Fact]
        public void Write_WithCrlfNewLineSetting_TrimsWhitespaceOnlyValueLine()
        {
            var builder = new CodeBuilder(new CodeBuilderSettings(newLine: "\r\n"));
            var body = "a\n \nb";

            builder.Write($"    {body}");

            Assert.Equal("    a\r\n\r\n    b", builder.ToString());
        }

        [Fact]
        public void Write_WithTrimDisabled_KeepsWhitespaceOnlyValueLine()
        {
            var builder = new CodeBuilder(
                new CodeBuilderSettings(trimWhitespaceOnlyLines: false));
            var body = "a\n \nb";

            builder.Write($"    {body}");

            Assert.Equal("    a\n     \n    b", builder.ToString());
        }

        [Fact]
        public void Write_WithSettingsPrefixParts_AppliesToWrite()
        {
            var builder = new CodeBuilder(
                new CodeBuilderSettings(prefixParts: PreservedPrefixParts.None));
            var body = "a\nb";

            builder.Write($"\t- {body}");

            Assert.Equal("\t- a\n   b", builder.ToString());
        }

        [Fact]
        public void Constructor_WithNullSettings_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new CodeBuilder((CodeBuilderSettings)null!));
        }

        [Fact]
        public void WriteLine_WithoutText_AppendsNewLine()
        {
            var builder = new CodeBuilder();

            builder.WriteLine();

            Assert.Equal("\n", builder.ToString());
        }

        [Fact]
        public void WriteLine_WithText_AppendsTextAndNewLine()
        {
            var builder = new CodeBuilder();

            builder.WriteLine($"a();");

            Assert.Equal("a();\n", builder.ToString());
        }

        [Fact]
        public void WriteLine_WithMultilineValue_ReindentsAndEndsLine()
        {
            var builder = new CodeBuilder();
            var body = "a();\nb();";

            builder.WriteLine($"    {body}");

            Assert.Equal("    a();\n    b();\n", builder.ToString());
        }

        [Fact]
        public void WriteLine_WithCrlfNewLineSetting_EmitsCrlf()
        {
            var builder = new CodeBuilder(new CodeBuilderSettings(newLine: "\r\n"));

            builder.WriteLine($"a");

            Assert.Equal("a\r\n", builder.ToString());
        }

        [Fact]
        public void WriteLine_AfterWhitespaceOnlyValueTail_TrimsLine()
        {
            var builder = new CodeBuilder();
            var body = "a\n ";

            builder.Write($"    {body}");
            builder.WriteLine();

            Assert.Equal("    a\n\n", builder.ToString());
        }

        [Fact]
        public void WriteLine_AfterLiteralIndentationAndWhitespaceOnlyValue_TrimsLine()
        {
            var builder = new CodeBuilder();
            var body = "  ";

            builder.Write($"\n    ");
            builder.Write($"{body}");
            builder.WriteLine();

            Assert.Equal("\n\n", builder.ToString());
        }

        [Fact]
        public void WriteLine_AfterLiteralIndentationAndValue_KeepsIndentation()
        {
            var builder = new CodeBuilder();
            var body = "x";

            builder.Write($"\n    ");
            builder.Write($"{body}");
            builder.WriteLine();

            Assert.Equal("\n    x\n", builder.ToString());
        }

        [Fact]
        public void WriteLine_WithNullText_ThrowsArgumentNullException()
        {
            var builder = new CodeBuilder();

            Assert.Throws<ArgumentNullException>(() => builder.WriteLine(null!));
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
        public void Write_WithFactoryCreatedString_RendersArguments()
        {
            var builder = new CodeBuilder();
            var text = FormattableStringFactory.Create("{0} and {1}", "a", "b");

            builder.Write(text);

            Assert.Equal("a and b", builder.ToString());
        }

        [Fact]
        public void Write_WithEmptyHole_ThrowsFormatException()
        {
            var builder = new CodeBuilder();
            var text = FormattableStringFactory.Create("{}");

            Assert.Throws<FormatException>(() => builder.Write(text));
        }

        [Fact]
        public void Write_WithNonDigitHole_ThrowsFormatException()
        {
            var builder = new CodeBuilder();
            var text = FormattableStringFactory.Create("{a}", "x");

            Assert.Throws<FormatException>(() => builder.Write(text));
        }

        [Fact]
        public void Write_WithUnterminatedHole_ThrowsFormatException()
        {
            var builder = new CodeBuilder();
            var text = FormattableStringFactory.Create("{0", "x");

            Assert.Throws<FormatException>(() => builder.Write(text));
        }

        [Fact]
        public void Write_WithTrailingOpenBrace_ThrowsFormatException()
        {
            var builder = new CodeBuilder();
            var text = FormattableStringFactory.Create("a{");

            Assert.Throws<FormatException>(() => builder.Write(text));
        }

        [Fact]
        public void Write_WithOutOfRangeIndex_ThrowsFormatException()
        {
            var builder = new CodeBuilder();
            var text = FormattableStringFactory.Create("{1}", "x");

            Assert.Throws<FormatException>(() => builder.Write(text));
        }

        [Fact]
        public void Write_WithUnmatchedClosingBrace_ThrowsFormatException()
        {
            var builder = new CodeBuilder();
            var text = FormattableStringFactory.Create("a}");

            Assert.Throws<FormatException>(() => builder.Write(text));
        }

        [Fact]
        public void ToString_WithoutWrites_ReturnsEmptyString()
        {
            var builder = new CodeBuilder();

            Assert.Equal(string.Empty, builder.ToString());
        }
    }
}
