namespace HlslTools.Syntax
{
    public class CompileExpressionSyntax : ExpressionSyntax
    {
        public readonly SyntaxToken CompileKeyword;
        public readonly SyntaxToken ShaderTargetToken;
        public readonly InvocationExpressionSyntax ShaderFunction;

        public CompileExpressionSyntax(SyntaxToken compileKeyword, SyntaxToken shaderTargetToken, InvocationExpressionSyntax shaderFunction)
            : base(SyntaxKind.InvocationExpression)
        {
            RegisterChildNode(out CompileKeyword, compileKeyword);
            RegisterChildNode(out ShaderTargetToken, shaderTargetToken);
            RegisterChildNode(out ShaderFunction, shaderFunction);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCompileExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCompileExpression(this);
        }
    }
}