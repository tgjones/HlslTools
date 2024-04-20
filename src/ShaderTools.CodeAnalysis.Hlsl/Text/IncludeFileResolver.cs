using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Text
{
    public sealed class IncludeFileResolver : IIncludeFileResolver
    {
        private readonly IIncludeFileSystem _fileSystem;
        private readonly HlslParseOptions _parserOptions;

        public IncludeFileResolver(IIncludeFileSystem fileSystem, HlslParseOptions parserOptions)
        {
            _fileSystem = fileSystem;
            _parserOptions = parserOptions;
        }

        public ImmutableArray<string> GetSearchDirectories(string includeFilename, SourceFile currentFile, bool isAngled)
        {
            var result = ImmutableArray.CreateBuilder<string>();

            if (!isAngled)
            {
                // Look through the hierarchy of files that included currentFile, to see if any of their
                // directories contain the include.
                var fileToCheck = currentFile;
                while (fileToCheck != null)
                {
                    if (fileToCheck.FilePath != null)
                    {
                        result.Add(Path.GetDirectoryName(fileToCheck.FilePath));
                    }
                    fileToCheck = fileToCheck.IncludedBy;
                }
            }

            // Then try additional include directories.
            foreach (var includeDirectory in _parserOptions.AdditionalIncludeDirectories)
            {
                result.Add(includeDirectory);
            }

            return result.ToImmutable();
        }

        public SourceFile OpenInclude(string includeFilename, SourceFile currentFile, bool isAngled)
        {
            SourceText text;

            // Resolve virtual directory mappings.
            includeFilename = MapIncludeWithVirtualDirectoryToRealPath(includeFilename);

            includeFilename = includeFilename
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            // Check for invalid path chars.
            if (includeFilename.Any(x => Path.GetInvalidPathChars().Contains(x)))
                return null;

            // If path is rooted, open it directly.
            if (Path.IsPathRooted(includeFilename))
            {
                if (_fileSystem.TryGetFile(includeFilename, out text))
                    return new SourceFile(text, currentFile, includeFilename);
                return null;
            }

            if (!isAngled)
            {
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
                            return new SourceFile(text, currentFile, testFilename);
                    }
                    fileToCheck = fileToCheck.IncludedBy;
                }
            }

            // Then try additional include directories.
            foreach (var includeDirectory in _parserOptions.AdditionalIncludeDirectories)
            {
                var testFilename = Path.Combine(includeDirectory, includeFilename);
                if (_fileSystem.TryGetFile(testFilename, out text))
                    return new SourceFile(text, currentFile, testFilename);
            }

            return null;
        }

        private string MapIncludeWithVirtualDirectoryToRealPath(string includeFilename)
        {
            foreach (var kvp in _parserOptions.VirtualDirectoryMappings)
            {
                if (includeFilename.StartsWith(kvp.Key))
                {
                    var virtualDirectory = includeFilename.Substring(0, kvp.Key.Length);
                    var remainingPath = includeFilename.Substring(virtualDirectory.Length + 1);
                    return Path.Combine(kvp.Value, remainingPath);
                }
            }

            return includeFilename;
        }
    }
}