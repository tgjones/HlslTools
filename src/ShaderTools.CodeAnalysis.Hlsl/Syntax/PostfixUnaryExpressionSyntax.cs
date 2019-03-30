using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed partial class PostfixUnaryExpressionSyntax : ExpressionSyntax
    {
        public PostfixUnaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax operand, SyntaxToken operatorToken)
            : this(kind, operand, operatorToken, SyntaxFacts.GetUnaryOperatorKind(kind))
        {
        }

        public PostfixUnaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax operand, SyntaxToken operatorToken, IEnumerable<Diagnostic> diagnostics)
            : this(kind, operand, operatorToken, SyntaxFacts.GetUnaryOperatorKind(kind), diagnostics)
        {
        }
    }
}