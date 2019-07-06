using Microsoft.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.BraceMatching
{
    internal struct BraceMatchingResult
    {
        public TextSpan LeftSpan { get; }
        public TextSpan RightSpan { get; }

        public BraceMatchingResult(TextSpan leftSpan, TextSpan rightSpan)
            : this()
        {
            this.LeftSpan = leftSpan;
            this.RightSpan = rightSpan;
        }
    }
}
