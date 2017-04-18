using ShaderTools.Core.Compilation;
using ShaderTools.Core.Syntax;
using ShaderTools.EditorServices.Workspace.Host;
using ShaderTools.EditorServices.Workspace.Host.Mef;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.EditorServices.Hlsl.LanguageServices
{
    [ExportLanguageService(typeof(ICompilationFactoryService), LanguageNames.Hlsl)]
    internal sealed class HlslCompilationFactoryService : ICompilationFactoryService
    {
        public CompilationBase CreateCompilation(SyntaxTreeBase syntaxTree)
        {
            return new Compilation((SyntaxTree) syntaxTree);
        }
    }
}
