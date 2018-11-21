using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public partial class AnnotationsSyntax : SyntaxNode
    {
        public AnnotationsSyntax(SyntaxToken lessThanToken, List<VariableDeclarationStatementSyntax> annotations, SyntaxToken greaterThanToken)
            : this(SyntaxKind.Annotations, lessThanToken, annotations, greaterThanToken)
        {
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
