using ShaderTools.Core.Compilation;
using ShaderTools.Core.Syntax;

namespace ShaderTools.EditorServices.Workspace.Host
{
    internal interface ICompilationFactoryService : ILanguageService
    {
        CompilationBase CreateCompilation(SyntaxTreeBase syntaxTree);
    }
}
