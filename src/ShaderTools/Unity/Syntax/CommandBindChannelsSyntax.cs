using System.Collections.Generic;

namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandBindChannelsSyntax : CommandSyntax
    {
        public readonly SyntaxToken BindChannelsKeyword;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<CommandSyntax> Commands;
        public readonly SyntaxToken CloseBraceToken;

        public CommandBindChannelsSyntax(SyntaxToken bindChannelsKeyword, SyntaxToken openBraceToken, List<CommandSyntax> commands, SyntaxToken closeBraceToken)
            : base(SyntaxKind.CommandBindChannels)
        {
            RegisterChildNode(out BindChannelsKeyword, bindChannelsKeyword);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Commands, commands);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandBindChannels(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandBindChannels(this);
        }
    }
}