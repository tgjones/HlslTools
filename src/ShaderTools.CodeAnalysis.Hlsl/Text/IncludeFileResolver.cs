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

        public ImmutableArray<string> GetSearchDirectories(string includeFilename, SourceFile currentFile)
        {
            var result = ImmutableArray.CreateBuilder<string>();

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

            // Then try additional include directories.
            foreach (var includeDirectory in _parserOptions.AdditionalIncludeDirectories)
            {
                result.Add(includeDirectory);
            }

            return result.ToImmutable();
        }

        public SourceFile OpenInclude(string includeFilename, SourceFile currentFile)
        {
            SourceText text;

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

            // Then try additional include directories.
            foreach (var includeDirectory in _parserOptions.AdditionalIncludeDirectories)
            {
                var testFilename = Path.Combine(includeDirectory, includeFilename);
                if (_fileSystem.TryGetFile(testFilename, out text))
                    return new SourceFile(text, currentFile, testFilename);
            }

            return null;
        }
    }
}