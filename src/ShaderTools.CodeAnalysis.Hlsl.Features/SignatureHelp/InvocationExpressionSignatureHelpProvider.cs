using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.SignatureHelp;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Symbols.Markup;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Collections;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.Hlsl.SignatureHelp
{
    internal abstract class InvocationExpressionSignatureHelpProvider<TNode> : AbstractHlslSignatureHelpProvider<TNode>
        where TNode : InvocationExpressionSyntax
    {
        public sealed override bool IsTriggerCharacter(char ch)
        {
            return ch == '(' || ch == ',';
        }

        public sealed override bool IsRetriggerCharacter(char ch)
        {
            return ch == ')';
        }

        protected sealed override SignatureHelpItems GetModel(
            SemanticModel semanticModel,
            TNode node,
            SourceLocation position)
        {
            var functionSymbols = GetFunctionSymbols(semanticModel, node, position).ToList();

            // try to bind to the actual method
            var symbolInfo = semanticModel.GetSymbolInfo(node);

            var selectedItem = symbolInfo.Symbol is FunctionSymbol fs
                ? functionSymbols.IndexOf(fs)
                : (int?)null;

            var signatureHelpItems = functionSymbols
                .Select(ConvertFunctionSymbol).ToList();

            var sourceRange = SignatureHelpUtilities.GetSignatureHelpSpan(node.ArgumentListSyntax);
            var textSpan = semanticModel.SyntaxTree.GetSourceFileSpan(sourceRange);

            var currentState = SignatureHelpUtilities.GetSignatureHelpState(node.ArgumentListSyntax, position);

            return CreateSignatureHelpItems(
                signatureHelpItems,
                textSpan,
                currentState,
                selectedItem);
        }

        protected abstract IEnumerable<FunctionSymbol> GetFunctionSymbols(
            SemanticModel semanticModel,
            TNode node,
            SourceLocation position);

        private SignatureHelpItem ConvertFunctionSymbol(FunctionSymbol function)
        {
            return CreateItem(
                function,
                false,
                c => !string.IsNullOrEmpty(function.Documentation)
                    ? ImmutableArray.Create(new TaggedText(TextTags.Text, function.Documentation))
                    : ImmutableArray<TaggedText>.Empty,
                GetMethodGroupPreambleParts(function),
                GetSeparatorParts(),
                GetMethodGroupPostambleParts(function),
                function.Parameters.Select(Convert).ToList()
            );
        }

        private static IList<SymbolMarkupToken> GetMethodGroupPreambleParts(FunctionSymbol method)
        {
            var result = new List<SymbolMarkupToken>();

            result.AddRange(method.ToMarkup(SymbolDisplayFormat.MinimallyQualifiedWithoutParameters).Tokens);
            result.Add(new SymbolMarkupToken(SymbolMarkupKind.Punctuation, "("));

            return result;
        }

        private static IList<SymbolMarkupToken> GetMethodGroupPostambleParts(FunctionSymbol method)
        {
            return SpecializedCollections.SingletonList(
                new SymbolMarkupToken(SymbolMarkupKind.Punctuation, ")"));
        }
    }
}