using Microsoft.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Hlsl.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.LanguageServices
{
    internal interface IWorkspaceIncludeFileSystem : IIncludeFileSystem, IWorkspaceService
    {
    }
}
