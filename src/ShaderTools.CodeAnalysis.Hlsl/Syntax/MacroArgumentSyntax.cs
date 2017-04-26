using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class MacroArgumentSyntax : SyntaxNode
    {
        public readonly List<SyntaxToken> Tokens;

        public MacroArgumentSyntax(List<SyntaxToken> tokens)
            : base(SyntaxKind.MacroArgument)
        {
            RegisterChildNodes(out Tokens, tokens);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitMacroArgument(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitMacroArgument(this);
        }
    }
}