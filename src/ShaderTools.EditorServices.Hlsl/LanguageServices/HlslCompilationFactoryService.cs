using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.EditorServices.Workspace.Host;
using ShaderTools.EditorServices.Workspace.Host.Mef;

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
