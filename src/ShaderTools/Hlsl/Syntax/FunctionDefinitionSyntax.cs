using System.Collections.Generic;

namespace ShaderTools.Hlsl.Syntax
{
    public sealed class FunctionDefinitionSyntax : FunctionSyntax
    {
        public readonly BlockSyntax Body;
        public readonly SyntaxToken SemicolonToken;

        public FunctionDefinitionSyntax(List<AttributeSyntax> attributes, List<SyntaxToken> modifiers, TypeSyntax returnType, DeclarationNameSyntax name, ParameterListSyntax parameterList, SemanticSyntax semantic, BlockSyntax body, SyntaxToken semicolonToken)
            : base(SyntaxKind.FunctionDefinition, attributes, modifiers, returnType, name, parameterList, semantic)
        {
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