using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class ParameterSyntax : SyntaxNode
    {
        public readonly List<SyntaxToken> Modifiers;
        public readonly TypeSyntax Type;
        public readonly VariableDeclaratorSyntax Declarator;

        public ParameterSyntax(List<SyntaxToken> modifiers, TypeSyntax type, VariableDeclaratorSyntax declarator)
            : base(SyntaxKind.Parameter)
        {
            RegisterChildNodes(out Modifiers, modifiers);
            RegisterChildNode(out Type, type);
            RegisterChildNode(out Declarator, declarator);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitParameter(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
           return visitor.VisitParameter(this);
        }
    }
}