using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public sealed class FunctionDefinitionSyntax : SyntaxNode
    {
        public readonly List<AttributeSyntax> Attributes;
        public readonly List<SyntaxToken> Modifiers;
        public readonly TypeSyntax ReturnType;
        public readonly DeclarationNameSyntax Name;
        public readonly ParameterListSyntax ParameterList;
        public readonly SemanticSyntax Semantic;
        public readonly BlockSyntax Body;
        public readonly SyntaxToken SemicolonToken;

        public FunctionDefinitionSyntax(List<AttributeSyntax> attributes, List<SyntaxToken> modifiers, TypeSyntax returnType, DeclarationNameSyntax name, ParameterListSyntax parameterList, SemanticSyntax semantic, BlockSyntax body, SyntaxToken semicolonToken)
            : base(SyntaxKind.FunctionDefinition)
        {
            RegisterChildNodes(out Attributes, attributes);
            RegisterChildNodes(out Modifiers, modifiers);
            RegisterChildNode(out ReturnType, returnType);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out ParameterList, parameterList);
            RegisterChildNode(out Semantic, semantic);
            RegisterChildNode(out Body, body);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitFunctionDefinition(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitFunctionDefinition(this);
        }
    }
}