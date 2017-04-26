using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class StateArrayInitializerSyntax : InitializerSyntax
    {
        public readonly SyntaxToken OpenBraceToken;
        public readonly SeparatedSyntaxList<StateInitializerSyntax> Initializers;
        public readonly SyntaxToken CloseBraceToken;

        public StateArrayInitializerSyntax(SyntaxToken openBraceToken, SeparatedSyntaxList<StateInitializerSyntax> initializers, SyntaxToken closeBraceToken)
            : base(SyntaxKind.StateArrayInitializer)
        {
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Initializers, initializers);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitStateArrayInitializer(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitStateArrayInitializer(this);
        }
    }
}