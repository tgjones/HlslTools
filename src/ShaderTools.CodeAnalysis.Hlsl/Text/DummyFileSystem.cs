using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Text
{
    internal sealed class DummyFileSystem : IIncludeFileSystem
    {
        public bool TryGetFile(string path, IncludeType includeType, out SourceText text)
        {
            text = null;
            return false;
        }
    }
}