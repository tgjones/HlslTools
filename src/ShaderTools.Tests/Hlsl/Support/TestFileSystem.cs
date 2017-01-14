using System.IO;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Tests.Hlsl.Support
{
    public sealed class TestFileSystem : IIncludeFileSystem
    {
        private readonly string _parentDirectory;

        public TestFileSystem(string parentFile)
        {
            _parentDirectory = Path.GetDirectoryName(parentFile);
        }

        public bool TryGetFile(string path, out SourceText text)
        {
            text = new StringText(File.ReadAllText(Path.Combine(_parentDirectory, path)), path);
            return true;
        }
    }
}