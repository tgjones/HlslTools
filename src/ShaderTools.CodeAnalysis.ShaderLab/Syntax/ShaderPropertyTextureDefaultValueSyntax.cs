using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class ShaderPropertyTextureDefaultValueSyntax : ShaderPropertyDefaultValueSyntax
    {
        public readonly SyntaxToken DefaultTextureToken;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<SyntaxToken> Options;
        public readonly SyntaxToken CloseBraceToken;

        public ShaderPropertyTextureDefaultValueSyntax(SyntaxToken defaultTextureToken, SyntaxToken openBraceToken, List<SyntaxToken> options, SyntaxToken closeBraceToken)
            : base(SyntaxKind.ShaderPropertyTextureDefaultValue)
        {
            RegisterChildNode(out DefaultTextureToken, defaultTextureToken);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Options, options);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShaderPropertyTextureDefaultValue(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShaderPropertyTextureDefaultValue(this);
        }
    }
}