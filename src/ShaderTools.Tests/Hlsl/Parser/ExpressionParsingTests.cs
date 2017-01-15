using System.Diagnostics;
using System.Linq;
using ShaderTools.Hlsl.Syntax;
using Xunit;

namespace ShaderTools.Tests.Hlsl.Parser
{
    public class ExpressionParsingTests
    {
        [Fact]
        public void TestName()
        {
            var text = "foo";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.IdentifierName, expr.Kind);
            Assert.False(((IdentifierNameSyntax)expr).Name.IsMissing);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
        }

        [Fact]
        public void TestParenthesizedExpression()
        {
            var text = "(foo)";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.ParenthesizedExpression, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
        }

        [Theory]
        [InlineData(SyntaxKind.TrueKeyword)]
        [InlineData(SyntaxKind.FalseKeyword)]
        public void TestPrimaryExpressions(SyntaxKind kind)
        {
            var text = kind.GetText();
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            var opKind = SyntaxFacts.GetLiteralExpression(kind);
            Assert.Equal(opKind, expr.Kind);
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var us = (LiteralExpressionSyntax)expr;
            Assert.NotNull(us.Token);
            Assert.Equal(kind, us.Token.Kind);
        }

        [Fact]
        public void TestStringLiteralExpression()
        {
            var text = "\"stuff\"";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.StringLiteralExpression, expr.Kind);
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var us = (StringLiteralExpressionSyntax)expr;
            Assert.Equal(us.Tokens.Count, 1);
            Assert.Equal(SyntaxKind.StringLiteralToken, us.Tokens[0].Kind);
        }

        [Fact]
        public void TestNumericLiteralExpression()
        {
            var text = "0";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.NumericLiteralExpression, expr.Kind);
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var us = (LiteralExpressionSyntax)expr;
            Assert.NotNull(us.Token);
            Assert.Equal(SyntaxKind.IntegerLiteralToken, us.Token.Kind);
        }

        [Theory]
        [InlineData(SyntaxKind.PlusToken)]
        [InlineData(SyntaxKind.MinusToken)]
        [InlineData(SyntaxKind.TildeToken)]
        [InlineData(SyntaxKind.NotToken)]
        [InlineData(SyntaxKind.PlusPlusToken)]
        [InlineData(SyntaxKind.MinusMinusToken)]
        public void TestPrefixUnaryOperators(SyntaxKind kind)
        {
            var text = kind.GetText() + "a";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            var opKind = SyntaxFacts.GetPrefixUnaryExpression(kind);
            Assert.Equal(opKind, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var us = (PrefixUnaryExpressionSyntax)expr;
            Assert.NotNull(us.OperatorToken);
            Assert.Equal(kind, us.OperatorToken.Kind);
            Assert.NotNull(us.Operand);
            Assert.Equal(SyntaxKind.IdentifierName, us.Operand.Kind);
            Assert.Equal("a", us.Operand.ToString());
        }

        [Theory]
        [InlineData(SyntaxKind.PlusPlusToken)]
        [InlineData(SyntaxKind.MinusMinusToken)]
        public void TestPostUnaryOperators(SyntaxKind kind)
        {
            var text = "a" + kind.GetText();
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            var opKind = SyntaxFacts.GetPostfixUnaryExpression(kind);
            Assert.Equal(opKind, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var us = (PostfixUnaryExpressionSyntax)expr;
            Assert.NotNull(us.OperatorToken);
            Assert.Equal(kind, us.OperatorToken.Kind);
            Assert.NotNull(us.Operand);
            Assert.Equal(SyntaxKind.IdentifierName, us.Operand.Kind);
            Assert.Equal("a", us.Operand.ToString());
        }

        [Theory]
        [InlineData(SyntaxKind.PlusToken)]
        [InlineData(SyntaxKind.MinusToken)]
        [InlineData(SyntaxKind.AsteriskToken)]
        [InlineData(SyntaxKind.SlashToken)]
        [InlineData(SyntaxKind.PercentToken)]
        [InlineData(SyntaxKind.EqualsEqualsToken)]
        [InlineData(SyntaxKind.ExclamationEqualsToken)]
        [InlineData(SyntaxKind.LessThanToken)]
        [InlineData(SyntaxKind.LessThanEqualsToken)]
        [InlineData(SyntaxKind.LessThanLessThanToken)]
        [InlineData(SyntaxKind.GreaterThanToken)]
        [InlineData(SyntaxKind.GreaterThanEqualsToken)]
        [InlineData(SyntaxKind.GreaterThanGreaterThanToken)]
        [InlineData(SyntaxKind.AmpersandToken)]
        [InlineData(SyntaxKind.AmpersandAmpersandToken)]
        [InlineData(SyntaxKind.BarToken)]
        [InlineData(SyntaxKind.BarBarToken)]
        [InlineData(SyntaxKind.CaretToken)]
        public void TestBinaryOperators(SyntaxKind kind)
        {
            var text = "(a) " + kind.GetText() + " b";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            var opKind = SyntaxFacts.GetBinaryExpression(kind);
            Assert.Equal(opKind, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var b = (BinaryExpressionSyntax)expr;
            Assert.NotNull(b.OperatorToken);
            Assert.Equal(kind, b.OperatorToken.Kind);
            Assert.NotNull(b.Left);
            Assert.NotNull(b.Right);
            Assert.Equal("(a)", b.Left.ToString());
            Assert.Equal("b", b.Right.ToString());
        }

        [Theory]
        [InlineData(SyntaxKind.PlusEqualsToken)]
        [InlineData(SyntaxKind.MinusEqualsToken)]
        [InlineData(SyntaxKind.AsteriskEqualsToken)]
        [InlineData(SyntaxKind.SlashEqualsToken)]
        [InlineData(SyntaxKind.PercentEqualsToken)]
        [InlineData(SyntaxKind.EqualsToken)]
        [InlineData(SyntaxKind.LessThanLessThanEqualsToken)]
        [InlineData(SyntaxKind.GreaterThanGreaterThanEqualsToken)]
        [InlineData(SyntaxKind.AmpersandEqualsToken)]
        [InlineData(SyntaxKind.BarEqualsToken)]
        [InlineData(SyntaxKind.CaretEqualsToken)]
        public void TestAssignmentOperators(SyntaxKind kind)
        {
            var text = "(a) " + kind.GetText() + " b";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            var opKind = SyntaxFacts.GetAssignmentExpression(kind);
            Assert.Equal(opKind, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var b = (AssignmentExpressionSyntax)expr;
            Assert.NotNull(b.OperatorToken);
            Assert.Equal(kind, b.OperatorToken.Kind);
            Assert.NotNull(b.Left);
            Assert.NotNull(b.Right);
            Assert.Equal("(a)", b.Left.ToString());
            Assert.Equal("b", b.Right.ToString());
        }

        [Fact]
        public void TestFieldAccess()
        {
            var kind = SyntaxKind.DotToken;
            var text = "(a)" + kind.GetText() + " b";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var e = (FieldAccessExpressionSyntax)expr;
            Assert.NotNull(e.DotToken);
            Assert.Equal(kind, e.DotToken.Kind);
            Assert.NotNull(e.Expression);
            Assert.NotNull(e.Name);
            Assert.Equal("(a)", e.Expression.ToString());
            Assert.Equal("b", e.Name.ToString());
        }

        [Fact]
        public void TestConditional()
        {
            var text = "a ? b : c";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.ConditionalExpression, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var ts = (ConditionalExpressionSyntax)expr;
            Assert.NotNull(ts.QuestionToken);
            Assert.NotNull(ts.ColonToken);
            Assert.Equal(SyntaxKind.QuestionToken, ts.QuestionToken.Kind);
            Assert.Equal(SyntaxKind.ColonToken, ts.ColonToken.Kind);
            Assert.Equal("a", ts.Condition.ToString());
            Assert.Equal("b", ts.WhenTrue.ToString());
            Assert.Equal("c", ts.WhenFalse.ToString());
        }

        [Fact]
        public void TestCast()
        {
            var text = "(a) b";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.CastExpression, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var cs = (CastExpressionSyntax)expr;
            Assert.NotNull(cs.OpenParenToken);
            Assert.NotNull(cs.CloseParenToken);
            Assert.False(cs.OpenParenToken.IsMissing);
            Assert.False(cs.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Type);
            Assert.NotNull(cs.Expression);
            Assert.Equal("a", cs.Type.ToString());
            Assert.Equal("b", cs.Expression.ToString());
        }

        [Fact]
        public void TestCastArrayConstVariableSize()
        {
            var text = "(uint[a]) b";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.CastExpression, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var cs = (CastExpressionSyntax) expr;
            Assert.NotNull(cs.OpenParenToken);
            Assert.NotNull(cs.CloseParenToken);
            Assert.False(cs.OpenParenToken.IsMissing);
            Assert.False(cs.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Type);
            Assert.NotNull(cs.Expression);
            Assert.Equal("uint", cs.Type.ToString());
            Assert.Equal(1, cs.ArrayRankSpecifiers.Count);
            Assert.Equal("[a]", cs.ArrayRankSpecifiers[0].ToString());
            Assert.Equal("b", cs.Expression.ToString());
        }

        [Fact]
        public void TestCastArrayLiteralSize()
        {
            var text = "(uint[4]) b";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.CastExpression, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var cs = (CastExpressionSyntax) expr;
            Assert.NotNull(cs.OpenParenToken);
            Assert.NotNull(cs.CloseParenToken);
            Assert.False(cs.OpenParenToken.IsMissing);
            Assert.False(cs.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Type);
            Assert.NotNull(cs.Expression);
            Assert.Equal("uint", cs.Type.ToString());
            Assert.Equal(1, cs.ArrayRankSpecifiers.Count);
            Assert.Equal("[4]", cs.ArrayRankSpecifiers[0].ToString());
            Assert.Equal("b", cs.Expression.ToString());
        }

        [Fact]
        public void TestNumericConstructorInvocation()
        {
            var text = "int(b)";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.NumericConstructorInvocationExpression, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var cs = (NumericConstructorInvocationExpressionSyntax)expr;
            Assert.NotNull(cs.ArgumentList.OpenParenToken);
            Assert.NotNull(cs.ArgumentList.CloseParenToken);
            Assert.False(cs.ArgumentList.OpenParenToken.IsMissing);
            Assert.False(cs.ArgumentList.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Type);
            Assert.Equal(1, cs.ArgumentList.Arguments.Count);
            Assert.Equal("int", cs.Type.ToString());
            Assert.Equal("b", cs.ArgumentList.Arguments[0].ToString());
        }

        [Fact]
        public void TestFunctionCallWithNoArgs()
        {
            var text = "a()";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.FunctionInvocationExpression, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var cs = (FunctionInvocationExpressionSyntax)expr;
            Assert.NotNull(cs.ArgumentList.OpenParenToken);
            Assert.NotNull(cs.ArgumentList.CloseParenToken);
            Assert.False(cs.ArgumentList.OpenParenToken.IsMissing);
            Assert.False(cs.ArgumentList.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Name);
            Assert.Equal(0, cs.ArgumentList.Arguments.Count);
            Assert.Equal(SyntaxKind.IdentifierName, cs.Name.Kind);
            Assert.Equal("a", ((IdentifierNameSyntax) cs.Name).Name.Text);
        }

        [Fact]
        public void TestMethodCallWithNoArgs()
        {
            var text = "a.b()";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.MethodInvocationExpression, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var cs = (MethodInvocationExpressionSyntax)expr;
            Assert.NotNull(cs.ArgumentList.OpenParenToken);
            Assert.NotNull(cs.ArgumentList.CloseParenToken);
            Assert.False(cs.ArgumentList.OpenParenToken.IsMissing);
            Assert.False(cs.ArgumentList.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Name);
            Assert.Equal(0, cs.ArgumentList.Arguments.Count);
            Assert.Equal("a", cs.Target.ToString());
            Assert.NotNull(cs.DotToken);
            Assert.Equal("b", cs.Name.Text);
        }

        [Fact]
        public void TestFunctionCallWithArgs()
        {
            var text = "a(b, c)";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.FunctionInvocationExpression, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var cs = (FunctionInvocationExpressionSyntax)expr;
            Assert.NotNull(cs.ArgumentList.OpenParenToken);
            Assert.NotNull(cs.ArgumentList.CloseParenToken);
            Assert.False(cs.ArgumentList.OpenParenToken.IsMissing);
            Assert.False(cs.ArgumentList.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Name);
            Assert.Equal(2, cs.ArgumentList.Arguments.Count);
            Assert.Equal(SyntaxKind.IdentifierName, cs.Name.Kind);
            Assert.Equal("a", ((IdentifierNameSyntax) cs.Name).Name.Text);
            Assert.Equal("b", cs.ArgumentList.Arguments[0].ToString());
            Assert.Equal("c", cs.ArgumentList.Arguments[1].ToString());
        }

        [Fact]
        public void TestMethodCallWithArgs()
        {
            var text = "d.a(b, c)";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.MethodInvocationExpression, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var cs = (MethodInvocationExpressionSyntax)expr;
            Assert.NotNull(cs.ArgumentList.OpenParenToken);
            Assert.NotNull(cs.ArgumentList.CloseParenToken);
            Assert.False(cs.ArgumentList.OpenParenToken.IsMissing);
            Assert.False(cs.ArgumentList.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Name);
            Assert.NotNull(cs.Target);
            Assert.NotNull(cs.DotToken);
            Assert.Equal(2, cs.ArgumentList.Arguments.Count);
            Assert.Equal("d", cs.Target.ToString());
            Assert.Equal("a", cs.Name.Text);
            Assert.Equal("b", cs.ArgumentList.Arguments[0].ToString());
            Assert.Equal("c", cs.ArgumentList.Arguments[1].ToString());
        }

        [Fact]
        public void TestIndex()
        {
            var text = "a[b]";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.ElementAccessExpression, expr.Kind);
            Assert.Equal(text, expr.ToString());
            Assert.Equal(0, expr.GetDiagnostics().Count());
            var ea = (ElementAccessExpressionSyntax)expr;
            Assert.NotNull(ea.OpenBracketToken);
            Assert.NotNull(ea.CloseBracketToken);
            Assert.False(ea.OpenBracketToken.IsMissing);
            Assert.False(ea.CloseBracketToken.IsMissing);
            Assert.NotNull(ea.Expression);
            Assert.Equal("a", ea.Expression.ToString());
            Assert.Equal("b", ea.Index.ToString());
        }

        [Fact]
        public void IndexingExpressionInParens()
        {
            var text = "(aRay[i])";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.ParenthesizedExpression, expr.Kind);

            var parenExp = (ParenthesizedExpressionSyntax)expr;
            Assert.Equal(SyntaxKind.ElementAccessExpression, parenExp.Expression.Kind);
        }

        [Fact]
        public void TestCommaOperator()
        {
            var text = "a = 0, b = 1, c(1, 2, (3, 2))";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.Equal(SyntaxKind.CompoundExpression, expr.Kind);

            var compoundExp = (CompoundExpressionSyntax)expr;
            Assert.Equal(SyntaxKind.CompoundExpression, compoundExp.Left.Kind);
            Assert.Equal(SyntaxKind.FunctionInvocationExpression, compoundExp.Right.Kind);

            var invocationExpr = (FunctionInvocationExpressionSyntax) compoundExp.Right;
            Assert.Equal(3, invocationExpr.ArgumentList.Arguments.Count);
            Assert.Equal("1", invocationExpr.ArgumentList.Arguments[0].ToString());
            Assert.Equal("2", invocationExpr.ArgumentList.Arguments[1].ToString());
            Assert.Equal("(3, 2)", invocationExpr.ArgumentList.Arguments[2].ToString());

            var leftCompoundExp = (CompoundExpressionSyntax)compoundExp.Left;
            Assert.Equal(SyntaxKind.SimpleAssignmentExpression, leftCompoundExp.Left.Kind);
            Assert.Equal(SyntaxKind.SimpleAssignmentExpression, leftCompoundExp.Right.Kind);
        }

        private static ExpressionSyntax ParseExpression(string text)
        {
            var expression = SyntaxFactory.ParseExpression(text).Root;

            foreach (var diagnostic in expression.GetDiagnostics())
                Debug.WriteLine(diagnostic.ToString());
            Assert.Equal(0, expression.GetDiagnostics().Count());
            Assert.Equal(text, expression.ToString());

            return (ExpressionSyntax) expression;
        }
    }
}