using Microsoft.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Host
{
    internal interface ICompilationFactoryService : ILanguageService
    {
        CompilationBase CreateCompilation(SyntaxTreeBase syntaxTree);
    }
}
