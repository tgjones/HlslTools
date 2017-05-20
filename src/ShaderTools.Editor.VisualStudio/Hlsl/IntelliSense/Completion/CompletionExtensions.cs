using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.Completion.CompletionProviders;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.Completion
{
    internal static class CompletionExtensions
    {
        public static CompletionModel GetCompletionModel(this SemanticModel semanticModel, int position, ITextSnapshot textSnapshot, IEnumerable<ICompletionProvider> providers, CancellationToken cancellationToken)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var rootFilePosition = semanticModel.Compilation.SyntaxTree.MapRootFilePosition(position);
            var token = GetIdentifierOrKeywordAtPosition((SyntaxNode) syntaxTree.Root, rootFilePosition);
            var applicableSpan = token?.FileSpan.Span ?? new TextSpan(position, 0);

            cancellationToken.ThrowIfCancellationRequested();

            var items = new List<CompletionItem>();
            foreach (var provider in providers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var item in provider.GetItems(semanticModel, rootFilePosition))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    items.Add(item);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            items.Sort(CompletionItemComparer.Instance);

            return new CompletionModel(semanticModel, applicableSpan, textSnapshot, items);
        }

        private static SyntaxToken GetIdentifierOrKeywordAtPosition(SyntaxNode root, SourceLocation position)
        {
            var syntaxToken = root.FindTokenOnLeft(position);
            return syntaxToken.Kind.IsIdentifierOrKeyword() && syntaxToken.SourceRange.ContainsOrTouches(position)
                ? syntaxToken
                : null;
        }

        private sealed class CompletionItemComparer : IComparer<CompletionItem>
        {
            public static readonly CompletionItemComparer Instance = new CompletionItemComparer();

            public int Compare(CompletionItem x, CompletionItem y)
            {
                return string.Compare(x.DisplayText, y.DisplayText, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}