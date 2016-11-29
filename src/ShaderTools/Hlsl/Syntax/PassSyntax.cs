using System.Collections.Generic;

namespace ShaderTools.Hlsl.Syntax
{
    public class PassSyntax : SyntaxNode
    {
        public readonly SyntaxToken PassKeyword;
        public readonly SyntaxToken Name;
        public readonly AnnotationsSyntax Annotations;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<StatementSyntax> Statements;
        public readonly SyntaxToken CloseBraceToken;

        public PassSyntax(SyntaxToken passKeyword, SyntaxToken name, AnnotationsSyntax annotations, SyntaxToken openBraceToken, List<StatementSyntax> statements, SyntaxToken closeBraceToken)
            : base(SyntaxKind.PassDeclaration)
        {
            RegisterChildNode(out PassKeyword, passKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out Annotations, annotations);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Statements, statements);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitPass(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitPass(this);
        }
    }
}