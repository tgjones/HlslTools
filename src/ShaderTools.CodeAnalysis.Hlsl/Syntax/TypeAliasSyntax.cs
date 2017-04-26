using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class TypeAliasSyntax : SyntaxNode
    {
        public readonly SyntaxToken Identifier;
        public readonly List<ArrayRankSpecifierSyntax> ArrayRankSpecifiers;
        public readonly List<VariableDeclaratorQualifierSyntax> Qualifiers;
        public readonly AnnotationsSyntax Annotations;

        public TypeAliasSyntax(SyntaxToken identifier, List<ArrayRankSpecifierSyntax> arrayRankSpecifiers, List<VariableDeclaratorQualifierSyntax> qualifiers, AnnotationsSyntax annotations)
            : base(SyntaxKind.TypeAlias)
        {
            RegisterChildNode(out Identifier, identifier);
            RegisterChildNodes(out ArrayRankSpecifiers, arrayRankSpecifiers);
            RegisterChildNodes(out Qualifiers, qualifiers);
            RegisterChildNode(out Annotations, annotations);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitTypeAlias(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitTypeAlias(this);
        }
    }
}