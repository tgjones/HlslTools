using System.Threading;
using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.ShaderLab.Syntax;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.ShaderLab.LanguageServices
{
    [ExportLanguageService(typeof(ISyntaxTreeFactoryService), LanguageNames.ShaderLab)]
    internal sealed class ShaderLabSyntaxTreeService : ISyntaxTreeFactoryService
    {
        public SyntaxTreeBase ParseSyntaxTree(SourceFile file, CancellationToken cancellationToken)
        {
            return SyntaxFactory.ParseUnitySyntaxTree(file.Text, cancellationToken);
        }
    }
}
