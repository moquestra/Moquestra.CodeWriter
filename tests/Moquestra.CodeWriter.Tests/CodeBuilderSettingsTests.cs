using System;

using Xunit;

namespace Moquestra.CodeWriter.Tests
{
    public class CodeBuilderSettingsTests
    {
        [Fact]
        public void Constructor_WithDefaults_UsesLineFeedTrimEnabledAndAllPrefixParts()
        {
            var settings = new CodeBuilderSettings();

            Assert.Equal("\n", settings.NewLine);
            Assert.True(settings.TrimWhitespaceOnlyValueLines);
            Assert.Equal(PreservedPrefixParts.All, settings.PrefixParts);
        }

        [Fact]
        public void Default_ReturnsSharedInstance()
        {
            Assert.Same(CodeBuilderSettings.Default, CodeBuilderSettings.Default);
        }

        [Fact]
        public void Constructor_WithNullNewLine_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new CodeBuilderSettings(newLine: null!));
        }

        [Fact]
        public void Constructor_WithUnsupportedNewLine_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new CodeBuilderSettings(newLine: "\r"));
        }

        [Fact]
        public void Constructor_WithUndefinedPrefixParts_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new CodeBuilderSettings(prefixParts: (PreservedPrefixParts)4));
        }
    }
}
