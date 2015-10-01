using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public class VariableDeclaratorSyntax : SyntaxNode
    {
        public readonly SyntaxToken Identifier;
        public readonly List<ArrayRankSpecifierSyntax> ArrayRankSpecifiers;
        public readonly List<VariableDeclaratorQualifierSyntax> Qualifiers;
        public readonly AnnotationsSyntax Annotations;
        public readonly InitializerSyntax Initializer;

        public VariableDeclaratorSyntax(SyntaxToken identifier, List<ArrayRankSpecifierSyntax> arrayRankSpecifiers, List<VariableDeclaratorQualifierSyntax> qualifiers, AnnotationsSyntax annotations, InitializerSyntax initializer)
            : base(SyntaxKind.VariableDeclarator)
        {
            RegisterChildNode(out Identifier, identifier);
            RegisterChildNodes(out ArrayRankSpecifiers, arrayRankSpecifiers);
            RegisterChildNodes(out Qualifiers, qualifiers);
            RegisterChildNode(out Annotations, annotations);
            RegisterChildNode(out Initializer, initializer);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitVariableDeclarator(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitVariableDeclarator(this);
        }
    }
}