using System.Threading;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;

namespace ShaderTools.EditorServices.Workspace.Host
{
    internal interface ISyntaxTreeFactoryService : ILanguageService
    {
        SyntaxTreeBase ParseSyntaxTree(SourceText text, CancellationToken cancellationToken);
    }
}
