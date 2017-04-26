using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class CompilationUnitSyntax : SyntaxNode
    {
        public readonly List<SyntaxNode> Declarations;
        public readonly SyntaxToken EndOfFileToken;

        public CompilationUnitSyntax(List<SyntaxNode> declarations, SyntaxToken endOfFileToken)
            : base(SyntaxKind.CompilationUnit)
        {
            RegisterChildNodes(out Declarations, declarations);
            RegisterChildNode(out EndOfFileToken, endOfFileToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCompilationUnit(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCompilationUnit(this);
        }
    }
}