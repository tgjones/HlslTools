using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandFogSyntax : CommandSyntax
    {
        public readonly SyntaxToken FogKeyword;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<CommandSyntax> Commands;
        public readonly SyntaxToken CloseBraceToken;

        public CommandFogSyntax(SyntaxToken fogKeyword, SyntaxToken openBraceToken, List<CommandSyntax> commands, SyntaxToken closeBraceToken)
            : base (SyntaxKind.CommandFog)
        {
            RegisterChildNode(out FogKeyword, fogKeyword);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Commands, commands);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandFog(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandFog(this);
        }
    }
}