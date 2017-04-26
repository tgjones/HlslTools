using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Text
{
    public sealed class IncludeFileResolver : IIncludeFileResolver
    {
        private readonly IIncludeFileSystem _fileSystem;

        public IncludeFileResolver(IIncludeFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public SourceText OpenInclude(string includeFilename, string currentFilename, string rootFilename, IEnumerable<string> additionalIncludeDirectories)
        {
            SourceText text;

            // Check for invalid path chars.
            if (includeFilename.Any(x => Path.GetInvalidPathChars().Contains(x)))
                return null;

            // If path is rooted, open it directly.
            if (Path.IsPathRooted(includeFilename))
            {
                _fileSystem.TryGetFile(includeFilename, out text);
                return text;
            }

            // If current file has been saved (i.e. has a filename), try same directory as current file.
            if (currentFilename != null)
            {
                var rootFileDirectory = Path.GetDirectoryName(currentFilename);
                if (_fileSystem.TryGetFile(Path.Combine(rootFileDirectory, includeFilename), out text))
                    return text;
            }

            // If this is not the root file, try same directory as root file.
            if (rootFilename != currentFilename)
            {
                var rootFileDirectory = Path.GetDirectoryName(rootFilename);
                if (_fileSystem.TryGetFile(Path.Combine(rootFileDirectory, includeFilename), out text))
                    return text;
            }

            // Then try additional include directories.
            foreach (var includeDirectory in additionalIncludeDirectories)
                if (_fileSystem.TryGetFile(Path.Combine(includeDirectory, includeFilename), out text))
                    return text;

            return null;
        }
    }
}