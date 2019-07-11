using System.Threading;
using Microsoft.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Host
{
    internal interface ISyntaxTreeFactoryService : ILanguageService
    {
        SyntaxTreeBase ParseSyntaxTree(SourceFile file, CancellationToken cancellationToken);
    }
}
