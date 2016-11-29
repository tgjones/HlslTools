using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Tests.Hlsl.Syntax
{
    [TestFixture]
    public class SyntaxTreeNavigationTests
    {
        [Test]
        public void TestGetPreviousTokenIncludingSkippedTokens()
        {
            var text =
@"cbuffer Globals {
  int a;
  garbage
  int b;
}";
            var tree = SyntaxFactory.ParseSyntaxTree(SourceText.From(text));
            Assert.AreEqual(text, tree.Root.ToFullString());

            var tokens = tree.Root.DescendantTokens(descendIntoTrivia: true).Where(t => t.Span.Length > 0).ToList();
            Assert.AreEqual(11, tokens.Count);
            Assert.AreEqual("garbage", tokens[6].Text);

            var list = new List<SyntaxToken>(tokens.Count);
            var token = tree.Root.GetLastToken(includeSkippedTokens: true);
            while (token != null)
            {
                list.Add(token);
                token = token.GetPreviousToken(includeSkippedTokens: true);
            }
            list.Reverse();

            Assert.AreEqual(tokens.Count, list.Count);
            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(list[i], tokens[i]);
            }
        }
    }
}