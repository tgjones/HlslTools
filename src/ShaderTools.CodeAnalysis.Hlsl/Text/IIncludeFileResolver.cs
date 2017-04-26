using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Text
{
    public interface IIncludeFileResolver
    {
        SourceText OpenInclude(string includeFilename, string currentFilename, string rootFilename, IEnumerable<string> additionalIncludeDirectories);
    }
}