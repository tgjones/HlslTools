using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public class AnnotationsSyntax : SyntaxNode
    {
        public readonly SyntaxToken LessThanToken;
        public readonly List<VariableDeclarationStatementSyntax> Annotations;
        public readonly SyntaxToken GreaterThanToken;

        public AnnotationsSyntax(SyntaxToken lessThanToken, List<VariableDeclarationStatementSyntax> annotations, SyntaxToken greaterThanToken)
            : base(SyntaxKind.Annotations)
        {
            RegisterChildNode(out LessThanToken, lessThanToken);
            RegisterChildNodes(out Annotations, annotations);
            RegisterChildNode(out GreaterThanToken, greaterThanToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitAnnotations(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitAnnotations(this);
        }
    }
}