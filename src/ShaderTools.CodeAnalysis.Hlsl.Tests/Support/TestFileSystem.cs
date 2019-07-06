using System.IO;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Support
{
    public sealed class TestFileSystem : IIncludeFileSystem
    {
        public bool TryGetFile(string path, out SourceText text)
        {
            if (File.Exists(path))
            {
                text = SourceText.From(File.ReadAllText(path));
                return true;
            }

            text = null;
            return false;
        }
    }
}