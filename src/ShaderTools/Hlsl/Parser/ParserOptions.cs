using System.Collections.Generic;

namespace ShaderTools.Hlsl.Parser
{
    public sealed class ParserOptions
    {
        public List<string> PreprocessorDefines { get; } = new List<string>();
    }
}