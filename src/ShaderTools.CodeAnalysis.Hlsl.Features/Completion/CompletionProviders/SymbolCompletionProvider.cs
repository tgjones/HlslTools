using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Completion;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Completion.Providers;
using ShaderTools.CodeAnalysis.Hlsl.Properties;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Symbols.Markup;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Completion.CompletionProviders
{
    internal sealed class SymbolCompletionProvider : CompletionProvider
    {
        internal override bool IsInsertionTrigger(SourceText text, int insertedCharacterPosition, OptionSet options)
        {
            return CompletionUtilities.IsTriggerCharacter(text, insertedCharacterPosition, options);
        }

        public override async Task ProvideCompletionsAsync(CompletionContext context)
        {
            var syntaxTree = (SyntaxTree) await context.Document.GetSyntaxTreeAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = (SemanticModel) await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var position = syntaxTree.MapRootFilePosition(context.Position);

            // We don't want to show a completions for these cases.
            if (syntaxTree.PossiblyInUserGivenName(position) || 
                syntaxTree.DefinitelyInMacro(position) || 
                syntaxTree.DefinitelyInVariableDeclaratorQualifier(position))
            {
                return;
            }

            var root = (SyntaxNode) syntaxTree.Root;

            // Comments and literals don't get completion information
            if (root.InComment(position) || root.InLiteral(position))
            {
                return;
            }

            if (syntaxTree.DefinitelyInTypeName(position))
            {
                GetTypeCompletions(semanticModel, position, context);
            }
            else
            {
                var propertyAccessExpression = GetPropertyAccessExpression(root, position);

                if (propertyAccessExpression == null)
                {
                    GetGlobalCompletions(semanticModel, position, context);
                }
                else
                {
                    GetMemberCompletions(semanticModel, propertyAccessExpression, context);
                }
            }
        }

        private static void GetTypeCompletions(SemanticModel semanticModel, SourceLocation position, CompletionContext context)
        {
            var symbols = semanticModel.LookupSymbols(position).OfType<TypeSymbol>();
            CreateSymbolCompletions(symbols, context);
        }

        private static void GetGlobalCompletions(SemanticModel semanticModel, SourceLocation position, CompletionContext context)
        {
            var symbols = semanticModel.LookupSymbols(position)
                .Where(x => !(x is SemanticSymbol))
                .Where(x => !(x is AttributeSymbol))
                .Where(x => x.Locations.Length == 0 || x.Locations.Any(l => l.End < position));

            if (!((SyntaxTree) semanticModel.SyntaxTree).PossiblyInTypeName(position))
                symbols = symbols.Where(x => !(x is TypeSymbol));

            CreateSymbolCompletions(symbols.Cast<Symbol>(), context);
        }

        private static void GetMemberCompletions(SemanticModel semanticModel, FieldAccessExpressionSyntax propertyAccessExpression, CompletionContext context)
        {
            var targetType = semanticModel.GetExpressionType(propertyAccessExpression.Expression);
            if (targetType != null && !targetType.IsUnknown() && !targetType.IsError())
            {
                GetTypeCompletions(targetType, context);
            }
        }

        private static void GetTypeCompletions(TypeSymbol targetType, CompletionContext context)
        {
            CreateSymbolCompletions(targetType.Members, context);
        }

        internal static FieldAccessExpressionSyntax GetPropertyAccessExpression(SyntaxNode root, SourceLocation position)
        {
            var token = root.FindTokenOnLeft(position);
            var previous = (SyntaxToken) token.GetPreviousToken(false, true);
            var dot = previous != null && previous.Kind == SyntaxKind.DotToken
                ? previous
                : token;

            var p = dot.Parent.AncestorsAndSelf().OfType<FieldAccessExpressionSyntax>().FirstOrDefault();

            if (p != null)
            {
                var afterDot = p.DotToken.SourceRange.End <= position && position <= p.Name.SourceRange.End;
                if (afterDot)
                    return p;
            }

            return null;
        }

        private static void CreateSymbolCompletions(IEnumerable<Symbol> symbols, CompletionContext context)
        {
            context.AddItems(symbols
                .GroupBy(s => s.Name)
                .Select(g => CreateSymbolCompletionGroup(g.Key, g.ToImmutableArray())));
        }

        private static CompletionItem CreateSymbolCompletionGroup(string name, ImmutableArray<Symbol> symbols)
        {
            var multiple = symbols.Skip(1).Any();
            if (!multiple)
                return CreateSymbolCompletion(symbols.First());

            var hasNonInvocables = symbols.Any(s => !(s is InvocableSymbol));
            if (!hasNonInvocables)
                return CreateInvocableCompletionGroup(symbols);

            if (symbols.All(s => (s is TypeSymbol && ((TypeSymbol) s).IsIntrinsicNumericType())
                                 || (s is FunctionSymbol && ((FunctionSymbol) s).IsNumericConstructor)))
                return CreateSymbolCompletion(symbols.First(s => s is TypeSymbol));

            var description = ImmutableArray.CreateBuilder<SymbolMarkupToken>();
            description.Add(new SymbolMarkupToken(SymbolMarkupKind.PlainText, HlslFeaturesResources.AmbiguousNamePrefix));
            foreach (var symbol in symbols)
            {
                description.Add(new SymbolMarkupToken(SymbolMarkupKind.Whitespace, "\n "));
                description.AddRange(symbol.ToMarkup().Tokens);
            }

            return CommonCompletionItem.Create(
                name,
                Glyph.CompletionWarning,
                description.ToImmutable());
        }

        private static CompletionItem CreateInvocableCompletionGroup(ImmutableArray<Symbol> symbols)
        {
            var symbol = symbols.First();
            var numberOfOverloads = symbols.Count() - 1;

            var overloadsSuffix = numberOfOverloads > 1
                ? string.Format(HlslFeaturesResources.CompletionItemWithOverloadsSuffix, numberOfOverloads)
                : HlslFeaturesResources.CompletionItemWithOverloadSuffix;

            return CreateSymbolCompletion(symbol, new SymbolMarkupToken(SymbolMarkupKind.PlainText, overloadsSuffix));
        }

        private static CompletionItem CreateSymbolCompletion(Symbol symbol, SymbolMarkupToken nameSuffix = null)
        {
            var displayText = symbol.Name;

            var description = symbol.ToMarkup();

            var descriptionTokens = description.Tokens;

            if (nameSuffix != null)
            {
                descriptionTokens = descriptionTokens.Add(nameSuffix);
            }

            if (!string.IsNullOrEmpty(symbol.Documentation))
            {
                descriptionTokens = descriptionTokens.Add(new SymbolMarkupToken(SymbolMarkupKind.Whitespace, "\n"));
                descriptionTokens = descriptionTokens.Add(new SymbolMarkupToken(SymbolMarkupKind.PlainText, symbol.Documentation));
            }

            var glyph = symbol.GetGlyph();

            return CommonCompletionItem.Create(
                displayText,
                glyph,
                descriptionTokens);
        }
    }
}
