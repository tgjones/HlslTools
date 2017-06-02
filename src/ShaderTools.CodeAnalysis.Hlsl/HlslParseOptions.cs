using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl
{
    public sealed class HlslParseOptions : ParseOptions
    {
        public Dictionary<string, string> PreprocessorDefines { get; } = new Dictionary<string, string>();
        public List<string> AdditionalIncludeDirectories { get; } = new List<string>();
    }
}