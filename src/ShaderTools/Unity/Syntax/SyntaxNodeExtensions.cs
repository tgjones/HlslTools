using System.Collections.Generic;
using ShaderTools.Core.Diagnostics;

namespace ShaderTools.Unity.Syntax
{
    public static class SyntaxNodeExtensions
    {
        public static IEnumerable<SyntaxToken> DescendantTokens(this SyntaxNode node, bool descendIntoTrivia = false)
        {
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.IsToken)
                {
                    var token = (SyntaxToken) childNode;
                    if (descendIntoTrivia)
                        foreach (var trivia in token.LeadingTrivia)
                            foreach (var descendantToken in trivia.DescendantTokens(true))
                                yield return descendantToken;
                    yield return (SyntaxToken) childNode;
                    if (descendIntoTrivia)
                        foreach (var trivia in token.TrailingTrivia)
                            foreach (var descendantToken in trivia.DescendantTokens(true))
                                yield return descendantToken;
                }
                else
                {
                    foreach (var descendantToken in childNode.DescendantTokens(descendIntoTrivia))
                        yield return descendantToken;
                }
            }
        }

        public static TNode WithDiagnostics<TNode>(this TNode node, IEnumerable<Diagnostic> diagnostics)
            where TNode : SyntaxNode
        {
            return (TNode)node.SetDiagnostics(node.Diagnostics.AddRange(diagnostics));
        }

        public static TNode WithDiagnostic<TNode>(this TNode node, Diagnostic diagnostic)
            where TNode : SyntaxNode
        {
            return (TNode)node.SetDiagnostics(node.Diagnostics.Add(diagnostic));
        }
    }
}