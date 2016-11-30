using System.Collections.Generic;

namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandSetTextureSyntax : CommandSyntax
    {
        public readonly SyntaxToken SetTextureKeyword;
        public readonly CommandVariableValueSyntax TextureName;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<CommandSyntax> Commands;
        public readonly SyntaxToken CloseBraceToken;

        public CommandSetTextureSyntax(SyntaxToken setTextureKeyword, CommandVariableValueSyntax textureName, SyntaxToken openBraceToken, List<CommandSyntax> commands, SyntaxToken closeBraceToken)
            : base(SyntaxKind.CommandSetTexture)
        {
            RegisterChildNode(out SetTextureKeyword, setTextureKeyword);
            RegisterChildNode(out TextureName, textureName);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Commands, commands);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandSetTexture(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandSetTexture(this);
        }
    }
}