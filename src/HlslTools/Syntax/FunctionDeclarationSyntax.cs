using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public class FunctionDeclarationSyntax : SyntaxNode
    {
        public readonly List<AttributeSyntax> Attributes;
        public readonly List<SyntaxToken> Modifiers;
        public readonly TypeSyntax ReturnType;
        public readonly SyntaxToken Name;
        public readonly ParameterListSyntax ParameterList;
        public readonly SemanticSyntax Semantic;
        public readonly SyntaxToken SemicolonToken;

        public FunctionDeclarationSyntax(List<AttributeSyntax> attributes, List<SyntaxToken> modifiers, TypeSyntax returnType, SyntaxToken name, ParameterListSyntax parameterList, SemanticSyntax semantic, SyntaxToken semicolonToken)
            : base(SyntaxKind.FunctionDeclaration)
        {
            RegisterChildNodes(out Attributes, attributes);
            RegisterChildNodes(out Modifiers, modifiers);
            RegisterChildNode(out ReturnType, returnType);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out ParameterList, parameterList);
            RegisterChildNode(out Semantic, semantic);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitFunctionDeclaration(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitFunctionDeclaration(this);
        }
    }
}