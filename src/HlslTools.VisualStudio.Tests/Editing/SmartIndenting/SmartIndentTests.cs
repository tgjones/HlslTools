using HlslTools.Syntax;
using HlslTools.Text;
using HlslTools.VisualStudio.Editing.SmartIndenting;
using NUnit.Framework;

namespace HlslTools.VisualStudio.Tests.Editing.SmartIndenting
{
    [TestFixture]
    public class SmartIndentTests
    {
        [Test]
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
            Assert.AreEqual(expectedIndent, actualIndent);
        }
    }
}