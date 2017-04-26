using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class ShaderPropertiesSyntax : SyntaxNode
    {
        public readonly SyntaxToken PropertiesKeyword;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<ShaderPropertySyntax> Properties;
        public readonly SyntaxToken CloseBraceToken;

        public ShaderPropertiesSyntax(SyntaxToken propertiesKeyword, SyntaxToken openBraceToken, List<ShaderPropertySyntax> properties, SyntaxToken closeBraceToken)
            : base(SyntaxKind.ShaderProperties)
        {
            RegisterChildNode(out PropertiesKeyword, propertiesKeyword);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Properties, properties);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShaderProperties(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShaderProperties(this);
        }
    }
}