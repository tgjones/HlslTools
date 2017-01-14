using System.IO;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Tests.Hlsl.Support
{
    public sealed class TestFileSystem : IIncludeFileSystem
    {
        public bool TryGetFile(string path, out SourceText text)
        {
            if (File.Exists(path))
            {
                text = new StringText(File.ReadAllText(path), path, isRoot: false);
                return true;
            }

            text = null;
            return false;
        }
    }
}