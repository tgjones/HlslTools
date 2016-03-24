using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public sealed class FunctionDeclarationSyntax : FunctionSyntax
    {
        public readonly SyntaxToken SemicolonToken;

        public FunctionDeclarationSyntax(List<AttributeSyntax> attributes, List<SyntaxToken> modifiers, TypeSyntax returnType, DeclarationNameSyntax name, ParameterListSyntax parameterList, SemanticSyntax semantic, SyntaxToken semicolonToken)
            : base(SyntaxKind.FunctionDeclaration, attributes, modifiers, returnType, name, parameterList, semantic)
        {
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