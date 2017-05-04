using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Parser
{
    internal sealed class FileSegment
    {
        public readonly SourceFile File;
        public readonly int Start;
        public int Length;

        public FileSegment(SourceFile file, int start)
        {
            File = file;
            Start = start;
        }
    }
}