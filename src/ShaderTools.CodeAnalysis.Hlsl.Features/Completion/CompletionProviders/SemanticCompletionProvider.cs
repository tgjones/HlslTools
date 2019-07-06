using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Completion;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Symbols.Markup;

namespace ShaderTools.CodeAnalysis.Hlsl.Completion.CompletionProviders
{
    internal sealed class SemanticCompletionProvider : CommonCompletionProvider
    {
        internal override bool IsInsertionTrigger(SourceText text, int insertedCharacterPosition, OptionSet options)
        {
            var ch = text[insertedCharacterPosition];

            return ch == ' ' || ch == ':';
        }

        public override async Task ProvideCompletionsAsync(CompletionContext context)
        {
            var syntaxTree = await context.Document.GetSyntaxTreeAsync(context.CancellationToken).ConfigureAwait(false);
            var sourceLocation = syntaxTree.MapRootFilePosition(context.Position);
            var token = ((SyntaxNode) syntaxTree.Root).FindTokenOnLeft(sourceLocation);

            var semanticNode = token.Parent.AncestorsAndSelf()
                .OfType<SemanticSyntax>()
                .FirstOrDefault();

            if (semanticNode == null || semanticNode.ColonToken.IsMissing || sourceLocation < semanticNode.ColonToken.SourceRange.End)
            {
                return;
            }

            // If semantic is used on a variable declaration inside a struct, try to use the name of the struct
            // to guess the shader type it is applied to.
            var parentStruct = semanticNode.Ancestors().OfType<StructTypeSyntax>().FirstOrDefault();
            var structUsage = GuessUsage(parentStruct);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var availableSemantics = semanticModel
                .LookupSymbols(semanticNode.Semantic.SourceRange.Start)
                .OfType<SemanticSymbol>();

            if (structUsage != SemanticUsages.None)
            {
                availableSemantics = availableSemantics.Where(x => x.Usages.HasFlag(structUsage));
            }

            foreach (var semantic in availableSemantics)
            {
                context.AddItem(CommonCompletionItem.Create(
                    $"{semantic.Name}{(semantic.AllowsMultiple ? "[n]" : "")}",
                    CompletionItemRules.Default,
                    Glyph.Constant,
                    CreateDescription(semantic)));
            }
        }

        private static ImmutableArray<SymbolMarkupToken> CreateDescription(SemanticSymbol semantic)
        {
            var builder = ImmutableArray.CreateBuilder<SymbolMarkupToken>();

            builder.Add(new SymbolMarkupToken(SymbolMarkupKind.PlainText, "(semantic) "));

            builder.Add(new SymbolMarkupToken(SymbolMarkupKind.SemanticName, semantic.Name));

            builder.Add(new SymbolMarkupToken(SymbolMarkupKind.Whitespace, Environment.NewLine));

            builder.Add(new SymbolMarkupToken(SymbolMarkupKind.PlainText, semantic.FullDescription));

            return builder.ToImmutable();
        }

        private static SemanticUsages GuessUsage(StructTypeSyntax structSyntax)
        {
            if (structSyntax == null || structSyntax.Name.IsMissing)
                return SemanticUsages.None;

            var structName = structSyntax.Name.Text;

            if (Contains(structName, "vs") || Contains(structName, "vertexshader"))
            {
                if (Contains(structName, "in") || Contains(structName, "input"))
                    return SemanticUsages.VertexShaderInput;
                if (Contains(structName, "out") || Contains(structName, "output"))
                    return SemanticUsages.VertexShaderOutput;
            }

            if (Contains(structName, "gs") || Contains(structName, "geometryshader"))
            {
                if (Contains(structName, "in") || Contains(structName, "input"))
                    return SemanticUsages.GeometryShaderInput;
                if (Contains(structName, "out") || Contains(structName, "output"))
                    return SemanticUsages.GeometryShaderOutput;
            }

            if (Contains(structName, "hs") || Contains(structName, "hullshader"))
            {
                if (Contains(structName, "in") || Contains(structName, "input"))
                    return SemanticUsages.HullShaderInput;
                if (Contains(structName, "out") || Contains(structName, "output"))
                    return SemanticUsages.HullShaderOutput;
            }

            if (Contains(structName, "ds") || Contains(structName, "domainshader"))
            {
                if (Contains(structName, "in") || Contains(structName, "input"))
                    return SemanticUsages.DomainShaderInput;
                if (Contains(structName, "out") || Contains(structName, "output"))
                    return SemanticUsages.DomainShaderOutput;
            }

            if (Contains(structName, "ps") || Contains(structName, "pixelshader"))
            {
                if (Contains(structName, "in") || Contains(structName, "input"))
                    return SemanticUsages.PixelShaderInput;
                if (Contains(structName, "out") || Contains(structName, "output"))
                    return SemanticUsages.PixelShaderOutput;
            }

            return SemanticUsages.None;
        }

        private static bool Contains(string text, string value)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(text, value, CompareOptions.IgnoreCase) != -1;
        }
    }
}
