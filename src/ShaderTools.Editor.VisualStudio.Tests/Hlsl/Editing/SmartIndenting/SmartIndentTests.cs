using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Editor.Hlsl.SmartIndent;
using ShaderTools.CodeAnalysis.Editor.Implementation.SmartIndent;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Editing.SmartIndenting
{
    public class SmartIndentTests : VisualStudioTestsBase
    {
        [Fact]
        public void TestIndent()
        {
            T(@"struct S
{
    |
};");
            T(@"struct S
{
    |
};
");
            T(@"void foo()
{
    if (true)
    {
        |
    }
}");
            T(@"void foo()
{
    if (true)
    {
        while (true)
        {
            |
        }
    }
}");
            T(@"struct S
{
    |
    float f;
    int i;
};");
            T(@"struct S
{
};
|
");
        }

        private static void T(string codeWithCaret)
        {
            int caret = codeWithCaret.IndexOf('|');
            int expectedIndent = 0;
            while (caret - expectedIndent > 1 && codeWithCaret[caret - expectedIndent - 1] == ' ')
            {
                expectedIndent++;
            }

            var indentationService = new HlslIndentationService();
            var syntaxFactsService = new HlslSyntaxFactsService();

            var code = codeWithCaret.Remove(caret, 1);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(SourceText.From(code)));
            var actualIndent = SmartIndent.FindTotalParentChainIndent((SyntaxNode) syntaxTree.Root, caret, 0, 4, indentationService, syntaxFactsService);
            Assert.Equal(expectedIndent, actualIndent);
        }
    }
}