using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandStencilSyntax : CommandSyntax
    {
        public readonly SyntaxToken StencilKeyword;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<CommandSyntax> StateProperties;
        public readonly SyntaxToken CloseBraceToken;

        public CommandStencilSyntax(SyntaxToken stencilKeyword, SyntaxToken openBraceToken, List<CommandSyntax> stateProperties, SyntaxToken closeBraceToken)
            : base(SyntaxKind.CommandStencil)
        {
            RegisterChildNode(out StencilKeyword, stencilKeyword);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out StateProperties, stateProperties);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandStencil(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandStencil(this);
        }
    }
}