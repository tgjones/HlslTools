using System;
using System.Diagnostics;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Parser
{
    public class IncludeDirectiveTests
    {
        [Theory]
        [InlineData("#include\"foo.fxh\"", true, "foo.fxh")]
        [InlineData("#include<foo.fxh>", false, "foo.fxh")]
        public void TestName(string text, bool isLocal, string trimmedName)
        {
            var expression = SyntaxFactory.ParseSyntaxTree(SourceText.From(text));
            var trivia = ((SyntaxToken)expression.Root.ChildNodes[0]).LeadingTrivia;

            Assert.Equal(1, trivia.Length);
                
            var includeTrivia = (IncludeDirectiveTriviaSyntax)trivia[0];

            Assert.NotNull(expression);
            Assert.Equal(SyntaxKind.IncludeDirectiveTrivia, includeTrivia.Kind);

            IncludeDirectiveTriviaSyntax directive = (IncludeDirectiveTriviaSyntax)includeTrivia;

            Assert.Equal(isLocal, directive.IsLocal);
            Assert.Equal(trimmedName, directive.TrimmedFilename);
        }
    }
}