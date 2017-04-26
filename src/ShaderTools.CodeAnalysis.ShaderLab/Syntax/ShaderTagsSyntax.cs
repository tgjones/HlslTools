using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class ShaderTagsSyntax : SyntaxNode
    {
        public readonly SyntaxToken TagsKeyword;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<ShaderTagSyntax> Tags;
        public readonly SyntaxToken CloseBraceToken;

        public ShaderTagsSyntax(SyntaxToken tagsKeyword, SyntaxToken openBraceToken, List<ShaderTagSyntax> tags, SyntaxToken closeBraceToken)
            : base(SyntaxKind.ShaderTags)
        {
            RegisterChildNode(out TagsKeyword, tagsKeyword);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Tags, tags);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShaderTags(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShaderTags(this);
        }
    }
}