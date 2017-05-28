using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.Editor.GoToDefinition;
using ShaderTools.CodeAnalysis.Editor.Host;
using ShaderTools.CodeAnalysis.FindUsages;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Editor.Hlsl.GoToDefinition
{
    [ExportLanguageService(typeof(IGoToDefinitionService), LanguageNames.Hlsl), Shared]
    internal sealed class HlslGoToDefinitionService : AbstractGoToDefinitionService
    {
        [ImportingConstructor]
        public HlslGoToDefinitionService([ImportMany] IEnumerable<Lazy<IStreamingFindUsagesPresenter>> streamingPresenters)
            : base(streamingPresenters)
        {
        }

        protected override async Task<bool> TryGoToSyntacticDefinitionAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var sourceLocation = syntaxTree.MapRootFilePosition(position);
            var syntaxFacts = document.GetLanguageService<ISyntaxFactsService>();
            var syntaxToken = (SyntaxToken) await syntaxTree.GetTouchingTokenAsync(sourceLocation, x => true, cancellationToken, findInsideTrivia: true).ConfigureAwait(false);

            if (syntaxToken == null)
            {
                return false;
            }

            if (syntaxToken.MacroReference == null)
            {
                return false;
            }

            var nameToken = syntaxToken.MacroReference.NameToken;

            if (!nameToken.SourceRange.ContainsOrTouches(sourceLocation))
            {
                return false;
            }

            if (!nameToken.FileSpan.IsInRootFile)
            {
                return false;
            }

            var definitionItem = DefinitionItem.Create(
                ImmutableArray<string>.Empty,
                ImmutableArray<TaggedText>.Empty,
                new DocumentSpan(document, syntaxToken.MacroReference.DefineDirective.MacroName.FileSpan),
                ImmutableArray<TaggedText>.Empty);

            return definitionItem.TryNavigateTo();
        }
    }
}
