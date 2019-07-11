using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.FindUsages;
using ShaderTools.CodeAnalysis.GoToDefinition;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Syntax;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.Hlsl.GoToDefinition
{
    [ExportLanguageService(typeof(IGoToDefinitionService), LanguageNames.Hlsl)]
    internal sealed class HlslGoToDefinitionService : AbstractGoToDefinitionService
    {
        protected override async Task<ImmutableArray<DefinitionItem>> GetSyntacticDefinitionsAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var sourceLocation = syntaxTree.MapRootFilePosition(position);
            var syntaxFacts = document.GetLanguageService<ISyntaxFactsService>();
            var syntaxToken = (SyntaxToken) await syntaxTree.GetTouchingTokenAsync(sourceLocation, x => true, cancellationToken, findInsideTrivia: true).ConfigureAwait(false);

            var empty = ImmutableArray<DefinitionItem>.Empty;

            if (syntaxToken == null)
            {
                return empty;
            }

            if (syntaxToken.MacroReference == null)
            {
                return empty;
            }

            var nameToken = syntaxToken.MacroReference.NameToken;

            if (!nameToken.SourceRange.ContainsOrTouches(sourceLocation))
            {
                return empty;
            }

            if (!nameToken.FileSpan.IsInRootFile)
            {
                return empty;
            }

            var definitionItem = DefinitionItem.Create(
                ImmutableArray<string>.Empty,
                ImmutableArray<TaggedText>.Empty,
                new DocumentSpan(document, syntaxToken.MacroReference.DefineDirective.MacroName.FileSpan),
                ImmutableArray<TaggedText>.Empty);

            return ImmutableArray.Create(definitionItem);
        }
    }
}
