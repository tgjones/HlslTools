using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using ShaderTools.Core.Symbols.Markup;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Symbols;
using ShaderTools.Hlsl.Symbols.Markup;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.SignatureHelp
{
    internal static class SignatureHelpExtensions
    {
        public static bool IsBetweenParentheses(this SyntaxNode node, SourceLocation position)
        {
            var argumentList = node.ChildNodes.SingleOrDefault(n => n.Kind == SyntaxKind.ArgumentList);
            if (argumentList != null)
                return argumentList.IsBetweenParentheses(position);

            // If there is a nested ArgumentList that contains this position, don't show signature help for this node type.
            if (node.DescendantNodes().Any(x => x.Kind == SyntaxKind.ArgumentList && x.IsBetweenParentheses(position)))
                return false;

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

        public static IEnumerable<SignatureItem> ToSignatureItems<TSymbol>(this IEnumerable<TSymbol> symbols)
            where TSymbol : InvocableSymbol
        {
            return symbols.Select(ToSignatureItem);
        }

        private static bool IsCommaToken(SymbolMarkupToken token)
        {
            return token.Kind == SymbolMarkupKind.Punctuation && token.Text == ",";
        }

        private static SignatureItem ToSignatureItem<TSymbol>(this TSymbol symbol, Func<SymbolMarkupToken, bool> separatorPredicate)
            where TSymbol : InvocableSymbol
        {
            var markup = symbol.ToMarkup();

            var sb = new StringBuilder();
            var parameterStart = 0;
            var nextNonWhitespaceStartsParameter = false;
            var parameterSpans = new List<TextSpan>();
            var parameterNamesAndDocs = new List<Tuple<string, string>>();

            foreach (var node in markup.Tokens)
            {
                var isParameterName = node.Kind == SymbolMarkupKind.ParameterName;
                var isWhitespace = node.Kind == SymbolMarkupKind.Whitespace;
                var isLeftParenthesis = node.Kind == SymbolMarkupKind.Punctuation && node.Text == "(";
                var isRightParenthesis = node.Kind == SymbolMarkupKind.Punctuation && node.Text == ")";
                var isSeparator = separatorPredicate(node);

                if (isParameterName)
                {
                    parameterNamesAndDocs.Add(Tuple.Create(node.Text, symbol.Parameters[parameterNamesAndDocs.Count].Documentation));
                }

                if (isLeftParenthesis)
                {
                    nextNonWhitespaceStartsParameter = true;
                }
                else if (isSeparator || isRightParenthesis)
                {
                    var end = sb.Length;
                    var span = TextSpan.FromBounds(null, parameterStart, end);
                    parameterSpans.Add(span);
                    nextNonWhitespaceStartsParameter = true;
                }
                else if (!isWhitespace && nextNonWhitespaceStartsParameter)
                {
                    parameterStart = sb.Length;
                    nextNonWhitespaceStartsParameter = false;
                }

                sb.Append(node.Text);
            }

            var parameters = parameterSpans
                .Zip(parameterNamesAndDocs, (s, n) => new ParameterItem(n.Item1, n.Item2, s))
                .ToImmutableArray();
            
            return new SignatureItem(symbol, sb.ToString(), symbol.Documentation, parameters);
        }

        private static SignatureItem ToSignatureItem<TSymbol>(this TSymbol symbol)
            where TSymbol : InvocableSymbol
        {
            return symbol.ToSignatureItem(IsCommaToken);
        }
    }
}