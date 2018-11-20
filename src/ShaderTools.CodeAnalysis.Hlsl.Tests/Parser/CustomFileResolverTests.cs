using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Parser
{
    public class CustomFileResolverTests
    {
        private class MyCustomResolver : IIncludeFileResolver
        {
            public string Path { get; private set; }

            public IncludeType IncludeType { get; private set; }

            public bool HasBeenCalled { get; private set; }

            public ImmutableArray<string> GetSearchDirectories(string includeFilename, SourceFile currentFile)
            {
                return new ImmutableArray<string>();
            }

            public SourceFile OpenInclude(string includeFilename, IncludeType includeType, SourceFile currentFile)
            {
                this.Path = includeFilename;
                this.IncludeType = includeType;
                this.HasBeenCalled = true;

                return new SourceFile(SourceText.From(""), currentFile);
            }

        }


        [Theory]
        [InlineData("#include\"foo.fxh\"", IncludeType.Local, "foo.fxh")]
        [InlineData("#include<foo.fxh>", IncludeType.System, "foo.fxh")]
        public void TestName(string text, IncludeType includeType, string trimmedName)
        {
            MyCustomResolver resolver = new MyCustomResolver();

            var expression = SyntaxFactory.ParseSyntaxTree(SourceText.From(text), null, null, resolver);

            Assert.True(resolver.HasBeenCalled);
            Assert.Equal(includeType, resolver.IncludeType);
            Assert.Equal(trimmedName, resolver.Path);
        }
    }
}