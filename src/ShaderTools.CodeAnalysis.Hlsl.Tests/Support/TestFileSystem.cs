using System.IO;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Support
{
    public sealed class TestFileSystem : IIncludeFileSystem
    {
        public bool TryGetFile(string path, IncludeType includeType, out SourceText text)
        {
            if (File.Exists(path))
            {
                text = new StringText(File.ReadAllText(path), path);
                return true;
            }

            text = null;
            return false;
        }
    }
}