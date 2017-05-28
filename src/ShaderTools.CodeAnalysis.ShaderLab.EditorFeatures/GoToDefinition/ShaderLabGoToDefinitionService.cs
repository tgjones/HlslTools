using System;
using System.Collections.Generic;
using System.Composition;
using ShaderTools.CodeAnalysis.Editor.GoToDefinition;
using ShaderTools.CodeAnalysis.Editor.Host;
using ShaderTools.CodeAnalysis.Host.Mef;

namespace ShaderTools.CodeAnalysis.Editor.ShaderLab.GoToDefinition
{
    [ExportLanguageService(typeof(IGoToDefinitionService), LanguageNames.ShaderLab), Shared]
    internal sealed class ShaderLabGoToDefinitionService : AbstractGoToDefinitionService
    {
        [ImportingConstructor]
        public ShaderLabGoToDefinitionService([ImportMany] IEnumerable<Lazy<IStreamingFindUsagesPresenter>> streamingPresenters)
            : base(streamingPresenters)
        {
        }
    }
}
