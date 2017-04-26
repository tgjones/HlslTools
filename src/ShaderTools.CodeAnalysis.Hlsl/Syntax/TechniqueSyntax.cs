using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class TechniqueSyntax : SyntaxNode
    {
        public readonly SyntaxToken TechniqueKeyword;
        public readonly SyntaxToken Name;
        public readonly AnnotationsSyntax Annotations;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<PassSyntax> Passes;
        public readonly SyntaxToken CloseBraceToken;
        public readonly SyntaxToken SemicolonToken;

        public TechniqueSyntax(SyntaxToken techniqueKeyword, SyntaxToken name, AnnotationsSyntax annotations, SyntaxToken openBraceToken, List<PassSyntax> passes, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
            : base(SyntaxKind.TechniqueDeclaration)
        {
            RegisterChildNode(out TechniqueKeyword, techniqueKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out Annotations, annotations);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Passes, passes);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitTechnique(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitTechnique(this);
        }
    }
}