namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandCustomEditorSyntax : CommandSyntax
    {
        public readonly SyntaxToken CustomEditorKeyword;
        public readonly SyntaxToken ValueToken;

        public CommandCustomEditorSyntax(SyntaxToken customEditorKeyword, SyntaxToken valueToken)
            : base(SyntaxKind.CommandCustomEditor)
        {
            RegisterChildNode(out CustomEditorKeyword, customEditorKeyword);
            RegisterChildNode(out ValueToken, valueToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandCustomEditor(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandCustomEditor(this);
        }
    }
}