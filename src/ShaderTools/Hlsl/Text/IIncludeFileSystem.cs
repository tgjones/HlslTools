using ShaderTools.Core.Text;

namespace ShaderTools.Hlsl.Text
{
    public interface IIncludeFileSystem
    {
        bool TryGetFile(string path, out SourceText text);
    }
}