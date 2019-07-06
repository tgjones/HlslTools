using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Tests.Support;
using ShaderTools.CodeAnalysis.Text;
using Xunit;
using Xunit.Abstractions;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Syntax
{
    public class SyntaxTreeTests
    {
        private readonly ITestOutputHelper _output;

        public SyntaxTreeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(33, 33)]
        [InlineData(34, 34)]
        [InlineData(35, 49)]
        [InlineData(36, 50)]
        [InlineData(40, 54)]
        [InlineData(60, 74)]
        public void TestMapRootFilePosition(int position, int expectedSourceLocation)
        {
            // Arrange.
            var syntaxTree = CreateSyntaxTree();

            // Act.
            var sourceLocation = syntaxTree.MapRootFilePosition(position);

            // Assert.
            Assert.Equal(expectedSourceLocation, sourceLocation.Position);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(75)]
        public void TestMapRootFilePositionWithInvalidArguments(int position)
        {
            // Arrange.
            var syntaxTree = CreateSyntaxTree();

            // Act / Assert.
            Assert.Throws<ArgumentOutOfRangeException>(() => syntaxTree.MapRootFilePosition(position));
        }

        [Theory]
        [InlineData(0, 0, 0, 0, true)]
        [InlineData(0, 1000, 0, 35, true)]
        [InlineData(1, 0, 1, 0, true)]
        [InlineData(1, 1000, 1, 34, true)]
        [InlineData(34, 1, 34, 1, true)]
        [InlineData(34, 2, 34, 1, true)]
        [InlineData(35, 0, 0, 0, false)]
        [InlineData(35, 1, 0, 1, false)]
        [InlineData(35, 1000, 0, 14, false)]
        [InlineData(36, 0, 1, 0, false)]
        [InlineData(36, 1, 1, 1, false)]
        [InlineData(47, 0, 12, 0, false)]
        [InlineData(48, 1, 13, 1, false)]
        [InlineData(48, 2, 13, 1, false)]
        [InlineData(49, 1, 35, 1, true)]
        [InlineData(73, 1, 59, 1, true)]
        [InlineData(74, 0, 60, 0, true)]
        [InlineData(74, 1, 60, 0, true)]
        public void TestGetSourceTextSpan(int start, int length, int expectedStart, int expectedLength, bool expectedIsRoot)
        {
            // Arrange.
            var syntaxTree = CreateSyntaxTree();
            var sourceRange = new SourceRange(new SourceLocation(start), length);

            // Act.
            var textSpan = syntaxTree.GetSourceFileSpan(sourceRange);

            // Assert.
            Assert.Equal(expectedStart, textSpan.Span.Start);
            Assert.Equal(expectedLength, textSpan.Span.Length);
            Assert.Equal(expectedIsRoot, textSpan.File.IsRootFile);
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(75, 0)]
        public void TestGetSourceTextSpanWithInvalidArguments(int start, int length)
        {
            // Arrange.
            var syntaxTree = CreateSyntaxTree();
            var sourceRange = new SourceRange(new SourceLocation(start), length);

            // Act / Assert.
            Assert.Throws<ArgumentOutOfRangeException>(() => syntaxTree.GetSourceFileSpan(sourceRange));
        }

        [Fact]
        public void CanParseWithConfiguredPreprocessorDefinition()
        {
            var code = @"
#if FOO == 1
float a;
#else
float b;
#endif";
            var options = new HlslParseOptions
            {
                PreprocessorDefines =
                {
                    { "FOO", "1" }
                }
            };
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(SourceText.From(code), "__Root__.hlsl"), options);

            foreach (var diagnostic in syntaxTree.GetDiagnostics())
                _output.WriteLine(diagnostic.ToString());
            Assert.Empty(syntaxTree.GetDiagnostics());
        }

        [Fact]
        public void CanParseWithInvalidConfiguredPreprocessorDefinition()
        {
            var code = @"
#if FOO == 1
float a;
#else
float b;
#endif";
            var options = new HlslParseOptions
            {
                PreprocessorDefines =
                {
                    { "FOO", "1 . % \\" }
                }
            };
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(SourceText.From(code), "__Root__.hlsl"), options);

            Assert.Equal(1, syntaxTree.GetDiagnostics().Count());
        }

        private static SyntaxTree CreateSyntaxTree()
        {
            // Arrange.
            const string fooText = @"
float baz;
";
            var fileSystem = new InMemoryFileSystem(new Dictionary<string, string>
            {
                { "foo.hlsl", fooText }
            });

            const string text = @"
float foo;
#include <foo.hlsl>
DECL(i, 2);
float bar;
";
            return SyntaxFactory.ParseSyntaxTree(new SourceFile(SourceText.From(text), "__Root__.hlsl"), fileSystem: fileSystem);
        }
    }
}
