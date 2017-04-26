using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.EditorServices.Workspace.Host
{
    internal interface ICompilationFactoryService : ILanguageService
    {
        CompilationBase CreateCompilation(SyntaxTreeBase syntaxTree);
    }
}
