using System.Threading;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;

namespace ShaderTools.EditorServices.Workspace.Host
{
    public interface ISyntaxTreeFactoryService : ILanguageService
    {
        SyntaxTreeBase ParseSyntaxTree(string filePath, SourceText text, CancellationToken cancellationToken);
    }
}
