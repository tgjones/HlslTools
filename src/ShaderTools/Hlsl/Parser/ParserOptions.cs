using System.Collections.Generic;

namespace ShaderTools.Hlsl.Parser
{
    public sealed class ParserOptions
    {
        public Dictionary<string, string> PreprocessorDefines { get; } = new Dictionary<string, string>();
        public List<string> AdditionalIncludeDirectories { get; } = new List<string>();
    }
}