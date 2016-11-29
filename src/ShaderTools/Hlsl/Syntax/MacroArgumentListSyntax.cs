namespace ShaderTools.Hlsl.Syntax
{
    public class MacroArgumentListSyntax : SyntaxNode
    {
        public readonly SyntaxToken OpenParenToken;
        public readonly SeparatedSyntaxList<MacroArgumentSyntax> Arguments;
        public readonly SyntaxToken CloseParenToken;

        public MacroArgumentListSyntax(SyntaxToken openParenToken, SeparatedSyntaxList<MacroArgumentSyntax> arguments, SyntaxToken closeParenToken)
            : base(SyntaxKind.ArgumentList)
        {
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNodes(out Arguments, arguments);
            RegisterChildNode(out CloseParenToken, closeParenToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitMacroArgumentList(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitMacroArgumentList(this);
        }
    }
}