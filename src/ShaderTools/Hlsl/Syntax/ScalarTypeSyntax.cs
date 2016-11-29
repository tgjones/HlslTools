using System.Collections.Generic;

namespace ShaderTools.Hlsl.Syntax
{
    public class ScalarTypeSyntax : NumericTypeSyntax
    {
        public readonly List<SyntaxToken> TypeTokens;

        public ScalarTypeSyntax(List<SyntaxToken> typeTokens)
            : base(SyntaxKind.PredefinedScalarType)
        {
            RegisterChildNodes(out TypeTokens, typeTokens);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitScalarType(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitScalarType(this);
        }
    }
}