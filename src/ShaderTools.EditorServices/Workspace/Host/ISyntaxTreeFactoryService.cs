using System.Threading;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.EditorServices.Workspace.Host
{
    internal interface ISyntaxTreeFactoryService : ILanguageService
    {
        SyntaxTreeBase ParseSyntaxTree(SourceText text, CancellationToken cancellationToken);
    }
}
