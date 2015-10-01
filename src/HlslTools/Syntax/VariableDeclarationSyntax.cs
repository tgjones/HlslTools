using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public class VariableDeclarationSyntax : SyntaxNode
    {
        public readonly List<SyntaxToken> Modifiers;
        public readonly TypeSyntax Type;
        public readonly SeparatedSyntaxList<VariableDeclaratorSyntax> Variables;

        public VariableDeclarationSyntax(List<SyntaxToken> modifiers, TypeSyntax type, SeparatedSyntaxList<VariableDeclaratorSyntax> variables)
            : base(SyntaxKind.VariableDeclaration)
        {
            RegisterChildNodes(out Modifiers, modifiers);
            RegisterChildNode(out Type, type);
            RegisterChildNodes(out Variables, variables);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitVariableDeclaration(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitVariableDeclaration(this);
        }
    }
}