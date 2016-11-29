using System.Collections.Generic;

namespace ShaderTools.Hlsl.Syntax
{
    public class StateInitializerSyntax : InitializerSyntax
    {
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<StatePropertySyntax> Properties;
        public readonly SyntaxToken CloseBraceToken;

        public StateInitializerSyntax(SyntaxToken openBraceToken, List<StatePropertySyntax> properties, SyntaxToken closeBraceToken)
            : base(SyntaxKind.StateInitializer)
        {
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Properties, properties);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitStateInitializer(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitStateInitializer(this);
        }
    }
}