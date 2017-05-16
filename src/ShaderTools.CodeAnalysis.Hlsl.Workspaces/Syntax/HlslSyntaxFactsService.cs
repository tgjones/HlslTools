using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    [ExportLanguageService(typeof(ISyntaxFactsService), LanguageNames.Hlsl)]
    internal sealed class HlslSyntaxFactsService : ISyntaxFactsService
    {
        public SourceFileSpan? GetFileSpanRoot(SyntaxNodeBase node)
        {
            return ((SyntaxNode) node).GetTextSpanRoot();
        }
    }
}
