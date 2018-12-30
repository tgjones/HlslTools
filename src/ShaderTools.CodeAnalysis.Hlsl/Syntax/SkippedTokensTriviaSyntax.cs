using System.Collections.Generic;
using System.Linq;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed partial class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
    {
        internal override bool IsSkippedTokensTrivia => true;

        public SkippedTokensTriviaSyntax(IEnumerable<SyntaxToken> tokens)
            : this(tokens.ToList())
        {
        }
    }
}