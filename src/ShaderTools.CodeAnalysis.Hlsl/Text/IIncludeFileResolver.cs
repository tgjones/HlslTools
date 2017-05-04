using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Text
{
    public interface IIncludeFileResolver
    {
        SourceFile OpenInclude(string includeFilename, SourceFile currentFile, IEnumerable<string> additionalIncludeDirectories);
    }
}