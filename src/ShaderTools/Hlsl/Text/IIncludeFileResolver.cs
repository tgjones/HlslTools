using System.Collections.Generic;
using ShaderTools.Core.Text;

namespace ShaderTools.Hlsl.Text
{
    public interface IIncludeFileResolver
    {
        SourceText OpenInclude(string includeFilename, string currentFilename, string rootFilename, IEnumerable<string> additionalIncludeDirectories);
    }
}