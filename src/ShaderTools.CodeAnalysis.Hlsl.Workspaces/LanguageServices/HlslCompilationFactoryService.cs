using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.LanguageServices
{
    [ExportLanguageService(typeof(ICompilationFactoryService), LanguageNames.Hlsl)]
    internal sealed class HlslCompilationFactoryService : ICompilationFactoryService
    {
        public CompilationBase CreateCompilation(SyntaxTreeBase syntaxTree)
        {
            return new Compilation.Compilation((SyntaxTree) syntaxTree);
        }
    }
}
