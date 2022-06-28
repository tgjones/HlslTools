using System.Collections.Generic;
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
                return OpenFileConsideringVirtualDirectory(includeFilename, currentFile);

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
                    var file = OpenFileConsideringVirtualDirectory(testFilename, currentFile);
                    if (file != null)
                        return file;
                }
                fileToCheck = fileToCheck.IncludedBy;
            }

            // Then try additional include directories.
            foreach (var includeDirectory in _parserOptions.AdditionalIncludeDirectories)
            {
                var testFilename = Path.Combine(includeDirectory, includeFilename);
                var file = OpenFileConsideringVirtualDirectory(testFilename, currentFile);
                if (file != null)
                    return file;
            }

            return null;
        }

        private static string RemoveDriveLetter(string path)
        {
            return Path.DirectorySeparatorChar + path.Substring(Path.GetPathRoot(path).Length);
        }

        private SourceFile TryOpenFileConsideringVirtualDirectory(string includeFileFulllPath, SourceFile currentFile)
        {
            // Resolve virtual directory mappings.
            includeFileFulllPath = MapIncludeWithVirtualDirectoryToRealPath(includeFileFulllPath, _parserOptions.VirtualDirectoryMappings) ?? includeFileFulllPath;

            SourceText text;
            if (!_fileSystem.TryGetFile(includeFileFulllPath, out text))
            {
                return null;
            }

            return new SourceFile(text, currentFile, includeFileFulllPath);
        }

        private SourceFile OpenFileConsideringVirtualDirectory(string includeFilename, SourceFile currentFile)
        {
            // Normalize the path
            var normalizedPath = Path.GetFullPath(includeFilename);
            var pathRoot = Path.GetPathRoot(includeFilename);
            if (pathRoot.Length == 1 && pathRoot[0] == Path.DirectorySeparatorChar)
            {
                // if includeFilename is an absolute path without drive letter, then normalize path should not contain drive letter
                includeFilename = RemoveDriveLetter(normalizedPath);
                var result = TryOpenFileConsideringVirtualDirectory(includeFilename, currentFile);
                if (result != null)
                {
                    return result;
                }
            }
            return TryOpenFileConsideringVirtualDirectory(normalizedPath, currentFile);
        }

        private string MapIncludeWithVirtualDirectoryToRealPath(string includeFilename, Dictionary<string, string> virtualDirectoryMappings)
        {
            if (!Path.IsPathRooted(includeFilename))
            {
                return null;
            }

            var parentVirtualDirectoryPath = Path.GetDirectoryName(includeFilename);
            var relativeVirtualDirectoryPath = Path.GetFileName(includeFilename);

            while (!string.IsNullOrEmpty(parentVirtualDirectoryPath))
            {
                if (virtualDirectoryMappings.TryGetValue(parentVirtualDirectoryPath, out string realDirectory))
                {
                    return Path.Combine(realDirectory, relativeVirtualDirectoryPath);
                }
                else
                {
                    relativeVirtualDirectoryPath = Path.Combine(Path.GetFileName(parentVirtualDirectoryPath), relativeVirtualDirectoryPath);
                    parentVirtualDirectoryPath = Path.GetDirectoryName(parentVirtualDirectoryPath);
                }
            }

            return null;
        }
    }
}