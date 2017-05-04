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

        public SourceFile OpenInclude(string includeFilename, SourceFile currentFile, IEnumerable<string> additionalIncludeDirectories)
        {
            SourceText text;

            // Check for invalid path chars.
            if (includeFilename.Any(x => Path.GetInvalidPathChars().Contains(x)))
                return null;

            // If path is rooted, open it directly.
            if (Path.IsPathRooted(includeFilename))
            {
                if (_fileSystem.TryGetFile(includeFilename, out text))
                    return new SourceFile(text, currentFile);
                return null;
            }

            // Look through the hierarchy of files that included currentFile, to see if any of their
            // directories contain the include.
            var fileToCheck = currentFile;
            while (fileToCheck != null)
            {
                if (fileToCheck.FilePath != null)
                {
                    var rootFileDirectory = Path.GetDirectoryName(fileToCheck.FilePath);
                    var testFilename = Path.Combine(rootFileDirectory, includeFilename);
                    if (_fileSystem.TryGetFile(testFilename, out text))
                        return new SourceFile(text, currentFile);
                }
                fileToCheck = fileToCheck.IncludedBy;
            }

            // Then try additional include directories.
            foreach (var includeDirectory in additionalIncludeDirectories)
            {
                var testFilename = Path.Combine(includeDirectory, includeFilename);
                if (_fileSystem.TryGetFile(testFilename, out text))
                    return new SourceFile(text, currentFile);
            }

            return null;
        }
    }
}