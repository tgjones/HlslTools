using System.IO;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Text
{
    internal sealed class VisualStudioFileSystem : IIncludeFileSystem
    {
        public bool TryGetFile(string path, out SourceText text)
        {
            // TODO: Look in running document table for requested file.

            if (File.Exists(path))
            {
                text = SourceText.From(File.ReadAllText(path), path);
                return true;
            }

            text = null;
            return false;
        }
    }
}