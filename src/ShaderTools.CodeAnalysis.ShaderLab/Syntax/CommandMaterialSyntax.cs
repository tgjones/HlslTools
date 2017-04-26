using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandMaterialSyntax : CommandSyntax
    {
        public readonly SyntaxToken MaterialKeyword;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<CommandSyntax> Commands;
        public readonly SyntaxToken CloseBraceToken;

        public CommandMaterialSyntax(SyntaxToken materialKeyword, SyntaxToken openBraceToken, List<CommandSyntax> commands, SyntaxToken closeBraceToken)
            : base (SyntaxKind.CommandMaterial)
        {
            RegisterChildNode(out MaterialKeyword, materialKeyword);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Commands, commands);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandMaterial(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandMaterial(this);
        }
    }
}