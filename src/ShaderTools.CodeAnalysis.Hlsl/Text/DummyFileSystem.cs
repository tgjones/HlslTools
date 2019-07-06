using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Text
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