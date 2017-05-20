using System.Collections.Generic;
using System.Linq;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
    {
        public readonly List<SyntaxToken> Tokens;

        internal override bool IsSkippedTokensTrivia => true;

        public SkippedTokensTriviaSyntax(IEnumerable<SyntaxToken> tokens)
            : base(SyntaxKind.SkippedTokensTrivia)
        {
            RegisterChildNodes(out Tokens, tokens.ToList());
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitSkippedTokensSyntaxTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitSkippedTokensSyntaxTrivia(this);
        }
    }
}