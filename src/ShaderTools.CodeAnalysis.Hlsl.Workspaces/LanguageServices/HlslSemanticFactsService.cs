using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.LanguageServices;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.LanguageServices
{
    [ExportLanguageService(typeof(ISemanticFactsService), LanguageNames.Hlsl)]
    internal sealed class HlslSemanticFactsService : ISemanticFactsService
    {
        public ISymbol GetDeclaredSymbol(SemanticModelBase semanticModel, ISyntaxToken token, CancellationToken cancellationToken)
        {
            var location = token.SourceRange;
            var q = from node in ((SyntaxToken) token).Ancestors()
                    let symbol = semanticModel.GetDeclaredSymbol(node)
                    where symbol != null && symbol.Locations.Contains(location)
                    select symbol;

            return q.FirstOrDefault();
        }
    }
}
