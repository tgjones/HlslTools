using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.Text;
using HlslTools.VisualStudio.IntelliSense.Completion.CompletionProviders;
using HlslTools.VisualStudio.Text;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.IntelliSense.Completion
{
    internal static class CompletionExtensions
    {
        public static CompletionModel GetCompletionModel(this SemanticModel semanticModel, int position, ITextSnapshot textSnapshot, IEnumerable<ICompletionProvider> providers)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var rootFilePosition = semanticModel.Compilation.SyntaxTree.MapRootFilePosition(position);
            var token = GetIdentifierOrKeywordAtPosition(syntaxTree.Root, rootFilePosition);
            var applicableSpan = token?.Span ?? new TextSpan(textSnapshot.ToSourceText(), position, 0);

            var items = providers.SelectMany(p => p.GetItems(semanticModel, rootFilePosition));
            var sortedItems = items.OrderBy(c => c.DisplayText).ToImmutableArray();

            return new CompletionModel(semanticModel, applicableSpan, textSnapshot, sortedItems);
        }

        private static SyntaxToken GetIdentifierOrKeywordAtPosition(SyntaxNode root, SourceLocation position)
        {
            var syntaxToken = root.FindTokenOnLeft(position);
            return syntaxToken.Kind.IsIdentifierOrKeyword() && syntaxToken.SourceRange.ContainsOrTouches(position)
                ? syntaxToken
                : null;
        }
    }
}