using System.IO;
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

        public SourceText GetInclude(string path)
        {
            return new StringText(File.ReadAllText(Path.Combine(_parentDirectory, path)), path);
        }
    }
}