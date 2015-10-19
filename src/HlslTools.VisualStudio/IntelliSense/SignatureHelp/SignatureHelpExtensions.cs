using System;
using System.Collections.Generic;
using System.Linq;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp
{
    internal static class SignatureHelpExtensions
    {
        public static bool IsBetweenParentheses(this SyntaxNode node, SourceLocation position)
        {
            var argumentList = node.ChildNodes.SingleOrDefault(n => n.Kind == SyntaxKind.ArgumentList);
            if (argumentList != null)
                return argumentList.IsBetweenParentheses(position);

            var leftParenthesisToken = node.GetSingleChildToken(SyntaxKind.OpenParenToken);
            var rightParenthesisToken = node.GetSingleChildToken(SyntaxKind.CloseParenToken);
            var start = leftParenthesisToken.IsMissing ? leftParenthesisToken.SourceRange.Start : leftParenthesisToken.SourceRange.End;
            var end = rightParenthesisToken.IsMissing ? node.FullSourceRange.End : rightParenthesisToken.SourceRange.Start;
            return start <= position && position <= end;
        }

        private static SyntaxToken GetSingleChildToken(this SyntaxNode node, SyntaxKind tokenKind)
        {
            return (SyntaxToken) node.ChildNodes.Where(x => x.IsToken).Single(nt => nt.Kind == tokenKind);
        }

        public static int GetParameterIndex(this ArgumentListSyntax argumentList, SourceLocation position)
        {
            var separators = argumentList.Arguments.GetSeparators();
            return separators.TakeWhile(s => !s.IsMissing && s.SourceRange.End <= position).Count();
        }

        public static IEnumerable<SignatureItem> ToSignatureItems(this IEnumerable<Symbol> symbols)
        {
            return symbols.Select(ToSignatureItem);
        }

        private static SignatureItem ToSignatureItem(this Symbol symbol)
        {
            throw new NotImplementedException();
            //return SymbolMarkup.ForSymbol(symbol).ToSignatureItem(IsCommaToken);
        }
    }
}