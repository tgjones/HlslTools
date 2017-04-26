using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class ShaderPropertySyntax : SyntaxNode
    {
        public readonly List<ShaderPropertyAttributeSyntax> Attributes;
        public readonly SyntaxToken NameToken;
        public readonly SyntaxToken OpenParenToken;
        public readonly SyntaxToken DisplayNameToken;
        public readonly SyntaxToken CommaToken;
        public readonly ShaderPropertyTypeSyntax PropertyType;
        public readonly SyntaxToken CloseParenToken;
        public readonly SyntaxToken EqualsToken;
        public readonly ShaderPropertyDefaultValueSyntax DefaultValue;

        public ShaderPropertySyntax(List<ShaderPropertyAttributeSyntax> attributes, SyntaxToken nameToken, SyntaxToken openParenToken, SyntaxToken displayNameToken, SyntaxToken commaToken, ShaderPropertyTypeSyntax propertyType, SyntaxToken closeParenToken, SyntaxToken equalsToken, ShaderPropertyDefaultValueSyntax defaultValue)
            : base(SyntaxKind.ShaderProperty)
        {
            RegisterChildNodes(out Attributes, attributes);
            RegisterChildNode(out NameToken, nameToken);
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out DisplayNameToken, displayNameToken);
            RegisterChildNode(out CommaToken, commaToken);
            RegisterChildNode(out PropertyType, propertyType);
            RegisterChildNode(out CloseParenToken, closeParenToken);
            RegisterChildNode(out EqualsToken, equalsToken);
            RegisterChildNode(out DefaultValue, defaultValue);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShaderProperty(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShaderProperty(this);
        }
    }
}