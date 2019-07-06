using Microsoft.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Text
{
    public interface IIncludeFileSystem
    {
        bool TryGetFile(string path, out SourceText text);
    }
}