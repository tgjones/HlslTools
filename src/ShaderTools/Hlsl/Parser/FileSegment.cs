using ShaderTools.Hlsl.Text;

namespace ShaderTools.Hlsl.Parser
{
    internal sealed class FileSegment
    {
        public readonly SourceText Text;
        public readonly int Start;
        public int Length;

        public FileSegment(SourceText text, int start)
        {
            Text = text;
            Start = start;
        }
    }
}