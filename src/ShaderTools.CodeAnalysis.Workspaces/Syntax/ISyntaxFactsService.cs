using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Syntax
{
    internal interface ISyntaxFactsService : ILanguageService
    {
        SourceFileSpan? GetFileSpanRoot(SyntaxNodeBase node);

        string GetKindText(ushort kind);
    }
}
