using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Text
{
    public interface IIncludeFileResolver
    {
        ImmutableArray<string> GetSearchDirectories(string includeFilename, SourceFile currentFile);
        SourceFile OpenInclude(string includeFilename, IncludeType includeType, SourceFile currentFile);
    }
}