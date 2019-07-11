using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Editor.Host;
using ShaderTools.CodeAnalysis.GoToDefinition;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Editor.GoToDefinition
{
    [ExportWorkspaceService(typeof(IEditorGoToDefinitionService)), Shared]
    internal sealed class EditorGoToDefinitionService : IEditorGoToDefinitionService
    {
        private readonly IEnumerable<Lazy<IStreamingFindUsagesPresenter>> _streamingPresenters;

        [ImportingConstructor]
        public EditorGoToDefinitionService(
            [ImportMany] IEnumerable<Lazy<IStreamingFindUsagesPresenter>> streamingPresenters)
        {
            _streamingPresenters = streamingPresenters;
        }

        public bool TryGoToDefinition(Document document, int position, CancellationToken cancellationToken)
        {
            var goToDefinitionService = document.GetLanguageService<IGoToDefinitionService>();

            var definitions = goToDefinitionService
                .FindDefinitionsAsync(document, position, cancellationToken)
                .WaitAndGetResult(cancellationToken);

            return GoToDefinitionHelpers.TryGoToDefinition(
                definitions,
                _streamingPresenters,
                cancellationToken: cancellationToken);
        }
    }
}
