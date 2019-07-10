using System.Composition;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.QuickInfo;

namespace ShaderTools.CodeAnalysis.Hlsl.QuickInfo
{
    [ExportLanguageServiceFactory(typeof(QuickInfoService), LanguageNames.Hlsl), Shared]
    internal sealed class CSharpQuickInfoServiceFactory : ILanguageServiceFactory
    {
        [ImportingConstructor]
        public CSharpQuickInfoServiceFactory()
        {
        }

        public ILanguageService CreateLanguageService(HostLanguageServices languageServices)
        {
            return new HlslQuickInfoService(languageServices.WorkspaceServices.Workspace);
        }
    }

    internal sealed class HlslQuickInfoService : QuickInfoService
    {
        public HlslQuickInfoService(Workspace workspace)
            : base(workspace, LanguageNames.Hlsl)
        {
        }
    }
}
