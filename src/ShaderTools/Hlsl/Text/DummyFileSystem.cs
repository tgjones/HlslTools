using ShaderTools.Core.Text;

namespace ShaderTools.Hlsl.Text
{
    internal sealed class DummyFileSystem : IIncludeFileSystem
    {
        public bool TryGetFile(string path, out SourceText text)
        {
            text = null;
            return false;
        }
    }
}