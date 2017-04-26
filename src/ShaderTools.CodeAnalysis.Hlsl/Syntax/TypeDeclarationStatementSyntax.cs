using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class TypeDeclarationStatementSyntax : StatementSyntax
    {
        public readonly List<SyntaxToken> Modifiers;
        public readonly TypeDefinitionSyntax Type;
        public readonly SyntaxToken SemicolonToken;

        public TypeDeclarationStatementSyntax(List<SyntaxToken> modifiers, TypeDefinitionSyntax type, SyntaxToken semicolonToken)
            : base(SyntaxKind.TypeDeclarationStatement, new List<AttributeSyntax>())
        {
            RegisterChildNodes(out Modifiers, modifiers);
            RegisterChildNode(out Type, type);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitTypeDeclarationStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitTypeDeclarationStatement(this);
        }
    }
}