using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Text
{
    public interface IIncludeFileSystem
    {
        bool TryGetFile(string path, IncludeType includeType, out SourceText text);
    }
}