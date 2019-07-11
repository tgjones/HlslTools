// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.QuickInfo;
using ShaderTools.CodeAnalysis.Syntax;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.Hlsl.QuickInfo
{
    [ExportQuickInfoProvider(PredefinedQuickInfoProviderNames.Syntactic, LanguageNames.Hlsl)]
    internal class HlslSyntacticQuickInfoProvider : AbstractQuickInfoProvider
    {
        protected override async Task<QuickInfoContent> BuildContentAsync(Document document, ISyntaxToken token, CancellationToken cancellationToken)
        {
            var macroDefinitionNode = token.Parent as DefineDirectiveTriviaSyntax;
            if (macroDefinitionNode != null && macroDefinitionNode.MacroName == token)
            {
                return new QuickInfoContent(
                    LanguageNames.Hlsl,
                    Glyph.Macro,
                    ImmutableArray.Create(new TaggedText(TextTags.Text, $"(macro definition) {macroDefinitionNode}")),
                    ImmutableArray<TaggedText>.Empty);
            }

            var syntaxToken = (SyntaxToken) token;
            var macroReference = syntaxToken.MacroReference;
            if (macroReference != null && macroReference.SourceRange == token.SourceRange)
            {
                return new QuickInfoContent(
                    LanguageNames.Hlsl,
                    Glyph.Macro,
                    ImmutableArray.Create(new TaggedText(TextTags.Text, $"(macro reference) {macroReference.DefineDirective.ToString(true)}")),
                    ImmutableArray<TaggedText>.Empty);
            }

            return null;
        }
    }
}
