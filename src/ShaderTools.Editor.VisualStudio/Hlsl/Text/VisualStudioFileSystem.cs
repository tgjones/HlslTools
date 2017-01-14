using System.IO;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Text
{
    internal sealed class VisualStudioFileSystem : IIncludeFileSystem
    {
        private readonly VisualStudioSourceTextFactory _sourceTextFactory;
        private readonly string _parentDirectory;

        public VisualStudioFileSystem(VisualStudioSourceTextContainer textContainer, VisualStudioSourceTextFactory sourceTextFactory)
        {
            _sourceTextFactory = sourceTextFactory;
            if (textContainer.Filename != null)
                _parentDirectory = Path.GetDirectoryName(textContainer.Filename);
        }

        public bool TryGetFile(string path, out SourceText text)
        {
            // TODO: Look in running document table for requested file.

            if (File.Exists(path))
            {
                text = _sourceTextFactory.CreateSourceText(path);
                return true;
            }

            text = null;
            return false;
        }
    }
}