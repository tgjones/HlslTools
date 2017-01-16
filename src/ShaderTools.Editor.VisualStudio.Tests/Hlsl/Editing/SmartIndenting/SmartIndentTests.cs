using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Editor.VisualStudio.Hlsl.Editing.SmartIndenting;
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

            var code = codeWithCaret.Remove(caret, 1);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var actualIndent = SmartIndent.FindTotalParentChainIndent(syntaxTree.Root, caret, 0);
            Assert.Equal(expectedIndent, actualIndent);
        }
    }
}