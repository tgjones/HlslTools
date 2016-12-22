using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.BraceMatching
{
    internal struct BraceMatchingResult
    {
        public static readonly BraceMatchingResult None = new BraceMatchingResult();

        public BraceMatchingResult(TextSpan left, TextSpan right)
        {
            IsValid = true;
            Left = left;
            Right = right;
        }

        public bool IsValid { get; }
        public TextSpan Left { get; }
        public TextSpan Right { get; }
    }
}