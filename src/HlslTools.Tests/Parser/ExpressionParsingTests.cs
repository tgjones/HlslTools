using System.Linq;
using HlslTools.Syntax;
using NUnit.Framework;

namespace HlslTools.Tests.Parser
{
    [TestFixture]
    public class ExpressionParsingTests
    {
        [Test]
        public void TestName()
        {
            var text = "foo";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.IdentifierName, expr.Kind);
            Assert.False(((IdentifierNameSyntax)expr).Name.IsMissing);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
        }

        [Test]
        public void TestParenthesizedExpression()
        {
            var text = "(foo)";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.ParenthesizedExpression, expr.Kind);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
        }

        [TestCase(SyntaxKind.TrueKeyword)]
        [TestCase(SyntaxKind.FalseKeyword)]
        public void TestPrimaryExpressions(SyntaxKind kind)
        {
            var text = kind.GetText();
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            var opKind = SyntaxFacts.GetLiteralExpression(kind);
            Assert.AreEqual(opKind, expr.Kind);
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var us = (LiteralExpressionSyntax)expr;
            Assert.NotNull(us.Token);
            Assert.AreEqual(kind, us.Token.Kind);
        }

        [Test]
        public void TestStringLiteralExpression()
        {
            var text = "\"stuff\"";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.StringLiteralExpression, expr.Kind);
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var us = (StringLiteralExpressionSyntax)expr;
            Assert.AreEqual(us.Tokens.Count, 1);
            Assert.AreEqual(SyntaxKind.StringLiteralToken, us.Tokens[0].Kind);
        }

        [Test]
        public void TestNumericLiteralExpression()
        {
            var text = "0";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.NumericLiteralExpression, expr.Kind);
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var us = (LiteralExpressionSyntax)expr;
            Assert.NotNull(us.Token);
            Assert.AreEqual(SyntaxKind.IntegerLiteralToken, us.Token.Kind);
        }

        [TestCase(SyntaxKind.PlusToken)]
        [TestCase(SyntaxKind.MinusToken)]
        [TestCase(SyntaxKind.TildeToken)]
        [TestCase(SyntaxKind.NotToken)]
        [TestCase(SyntaxKind.PlusPlusToken)]
        [TestCase(SyntaxKind.MinusMinusToken)]
        public void TestPrefixUnaryOperators(SyntaxKind kind)
        {
            var text = kind.GetText() + "a";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            var opKind = SyntaxFacts.GetPrefixUnaryExpression(kind);
            Assert.AreEqual(opKind, expr.Kind);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var us = (PrefixUnaryExpressionSyntax)expr;
            Assert.NotNull(us.OperatorToken);
            Assert.AreEqual(kind, us.OperatorToken.Kind);
            Assert.NotNull(us.Operand);
            Assert.AreEqual(SyntaxKind.IdentifierName, us.Operand.Kind);
            Assert.AreEqual("a", us.Operand.ToString());
        }

        [TestCase(SyntaxKind.PlusPlusToken)]
        [TestCase(SyntaxKind.MinusMinusToken)]
        public void TestPostUnaryOperators(SyntaxKind kind)
        {
            var text = "a" + kind.GetText();
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            var opKind = SyntaxFacts.GetPostfixUnaryExpression(kind);
            Assert.AreEqual(opKind, expr.Kind);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var us = (PostfixUnaryExpressionSyntax)expr;
            Assert.NotNull(us.OperatorToken);
            Assert.AreEqual(kind, us.OperatorToken.Kind);
            Assert.NotNull(us.Operand);
            Assert.AreEqual(SyntaxKind.IdentifierName, us.Operand.Kind);
            Assert.AreEqual("a", us.Operand.ToString());
        }

        [TestCase(SyntaxKind.PlusToken)]
        [TestCase(SyntaxKind.MinusToken)]
        [TestCase(SyntaxKind.AsteriskToken)]
        [TestCase(SyntaxKind.SlashToken)]
        [TestCase(SyntaxKind.PercentToken)]
        [TestCase(SyntaxKind.EqualsEqualsToken)]
        [TestCase(SyntaxKind.ExclamationEqualsToken)]
        [TestCase(SyntaxKind.LessThanToken)]
        [TestCase(SyntaxKind.LessThanEqualsToken)]
        [TestCase(SyntaxKind.LessThanLessThanToken)]
        [TestCase(SyntaxKind.GreaterThanToken)]
        [TestCase(SyntaxKind.GreaterThanEqualsToken)]
        [TestCase(SyntaxKind.GreaterThanGreaterThanToken)]
        [TestCase(SyntaxKind.AmpersandToken)]
        [TestCase(SyntaxKind.AmpersandAmpersandToken)]
        [TestCase(SyntaxKind.BarToken)]
        [TestCase(SyntaxKind.BarBarToken)]
        [TestCase(SyntaxKind.CaretToken)]
        public void TestBinaryOperators(SyntaxKind kind)
        {
            var text = "(a) " + kind.GetText() + " b";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            var opKind = SyntaxFacts.GetBinaryExpression(kind);
            Assert.AreEqual(opKind, expr.Kind);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var b = (BinaryExpressionSyntax)expr;
            Assert.NotNull(b.OperatorToken);
            Assert.AreEqual(kind, b.OperatorToken.Kind);
            Assert.NotNull(b.Left);
            Assert.NotNull(b.Right);
            Assert.AreEqual("(a)", b.Left.ToString());
            Assert.AreEqual("b", b.Right.ToString());
        }

        [TestCase(SyntaxKind.PlusEqualsToken)]
        [TestCase(SyntaxKind.MinusEqualsToken)]
        [TestCase(SyntaxKind.AsteriskEqualsToken)]
        [TestCase(SyntaxKind.SlashEqualsToken)]
        [TestCase(SyntaxKind.PercentEqualsToken)]
        [TestCase(SyntaxKind.EqualsToken)]
        [TestCase(SyntaxKind.LessThanLessThanEqualsToken)]
        [TestCase(SyntaxKind.GreaterThanGreaterThanEqualsToken)]
        [TestCase(SyntaxKind.AmpersandEqualsToken)]
        [TestCase(SyntaxKind.BarEqualsToken)]
        [TestCase(SyntaxKind.CaretEqualsToken)]
        public void TestAssignmentOperators(SyntaxKind kind)
        {
            var text = "(a) " + kind.GetText() + " b";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            var opKind = SyntaxFacts.GetAssignmentExpression(kind);
            Assert.AreEqual(opKind, expr.Kind);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var b = (AssignmentExpressionSyntax)expr;
            Assert.NotNull(b.OperatorToken);
            Assert.AreEqual(kind, b.OperatorToken.Kind);
            Assert.NotNull(b.Left);
            Assert.NotNull(b.Right);
            Assert.AreEqual("(a)", b.Left.ToString());
            Assert.AreEqual("b", b.Right.ToString());
        }

        [Test]
        public void TestMemberAccess()
        {
            var kind = SyntaxKind.DotToken;
            var text = "(a)" + kind.GetText() + " b";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var e = (MemberAccessExpressionSyntax)expr;
            Assert.NotNull(e.DotToken);
            Assert.AreEqual(kind, e.DotToken.Kind);
            Assert.NotNull(e.Expression);
            Assert.NotNull(e.Name);
            Assert.AreEqual("(a)", e.Expression.ToString());
            Assert.AreEqual("b", e.Name.ToString());
        }

        [Test]
        public void TestConditional()
        {
            var text = "a ? b : c";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.ConditionalExpression, expr.Kind);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var ts = (ConditionalExpressionSyntax)expr;
            Assert.NotNull(ts.QuestionToken);
            Assert.NotNull(ts.ColonToken);
            Assert.AreEqual(SyntaxKind.QuestionToken, ts.QuestionToken.Kind);
            Assert.AreEqual(SyntaxKind.ColonToken, ts.ColonToken.Kind);
            Assert.AreEqual("a", ts.Condition.ToString());
            Assert.AreEqual("b", ts.WhenTrue.ToString());
            Assert.AreEqual("c", ts.WhenFalse.ToString());
        }

        [Test]
        public void TestCast()
        {
            var text = "(a) b";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.CastExpression, expr.Kind);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var cs = (CastExpressionSyntax)expr;
            Assert.NotNull(cs.OpenParenToken);
            Assert.NotNull(cs.CloseParenToken);
            Assert.False(cs.OpenParenToken.IsMissing);
            Assert.False(cs.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Type);
            Assert.NotNull(cs.Expression);
            Assert.AreEqual("a", cs.Type.ToString());
            Assert.AreEqual("b", cs.Expression.ToString());
        }

        [Test]
        public void TestNumericConstructorInvocation()
        {
            var text = "int(b)";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.NumericConstructorInvocationExpression, expr.Kind);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var cs = (NumericConstructorInvocationExpressionSyntax)expr;
            Assert.NotNull(cs.ArgumentList.OpenParenToken);
            Assert.NotNull(cs.ArgumentList.CloseParenToken);
            Assert.False(cs.ArgumentList.OpenParenToken.IsMissing);
            Assert.False(cs.ArgumentList.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Type);
            Assert.AreEqual(1, cs.ArgumentList.Arguments.Count);
            Assert.AreEqual("int", cs.Type.ToString());
            Assert.AreEqual("b", cs.ArgumentList.Arguments[0].ToString());
        }

        [Test]
        public void TestCallWithNoArgs()
        {
            var text = "a()";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.InvocationExpression, expr.Kind);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var cs = (InvocationExpressionSyntax)expr;
            Assert.NotNull(cs.ArgumentList.OpenParenToken);
            Assert.NotNull(cs.ArgumentList.CloseParenToken);
            Assert.False(cs.ArgumentList.OpenParenToken.IsMissing);
            Assert.False(cs.ArgumentList.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Expression);
            Assert.AreEqual(0, cs.ArgumentList.Arguments.Count);
            Assert.AreEqual("a", cs.Expression.ToString());
        }

        [Test]
        public void TestCallWithArgs()
        {
            var text = "a(b, c)";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.InvocationExpression, expr.Kind);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var cs = (InvocationExpressionSyntax)expr;
            Assert.NotNull(cs.ArgumentList.OpenParenToken);
            Assert.NotNull(cs.ArgumentList.CloseParenToken);
            Assert.False(cs.ArgumentList.OpenParenToken.IsMissing);
            Assert.False(cs.ArgumentList.CloseParenToken.IsMissing);
            Assert.NotNull(cs.Expression);
            Assert.AreEqual(2, cs.ArgumentList.Arguments.Count);
            Assert.AreEqual("a", cs.Expression.ToString());
            Assert.AreEqual("b", cs.ArgumentList.Arguments[0].ToString());
            Assert.AreEqual("c", cs.ArgumentList.Arguments[1].ToString());
        }

        [Test]
        public void TestIndex()
        {
            var text = "a[b]";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.ElementAccessExpression, expr.Kind);
            Assert.AreEqual(text, expr.ToString());
            Assert.AreEqual(0, expr.GetDiagnostics().Count());
            var ea = (ElementAccessExpressionSyntax)expr;
            Assert.NotNull(ea.OpenBracketToken);
            Assert.NotNull(ea.CloseBracketToken);
            Assert.False(ea.OpenBracketToken.IsMissing);
            Assert.False(ea.CloseBracketToken.IsMissing);
            Assert.NotNull(ea.Expression);
            Assert.AreEqual("a", ea.Expression.ToString());
            Assert.AreEqual("b", ea.Index.ToString());
        }

        [Test]
        public void IndexingExpressionInParens()
        {
            var text = "(aRay[i])";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.ParenthesizedExpression, expr.Kind);

            var parenExp = (ParenthesizedExpressionSyntax)expr;
            Assert.AreEqual(SyntaxKind.ElementAccessExpression, parenExp.Expression.Kind);
        }

        [Test]
        public void TestCommaOperator()
        {
            var text = "a = 0, b = 1, c(1, 2, (3, 2))";
            var expr = ParseExpression(text);

            Assert.NotNull(expr);
            Assert.AreEqual(SyntaxKind.CompoundExpression, expr.Kind);

            var compoundExp = (CompoundExpressionSyntax)expr;
            Assert.AreEqual(SyntaxKind.CompoundExpression, compoundExp.Left.Kind);
            Assert.AreEqual(SyntaxKind.InvocationExpression, compoundExp.Right.Kind);

            var invocationExpr = (InvocationExpressionSyntax) compoundExp.Right;
            Assert.AreEqual(3, invocationExpr.ArgumentList.Arguments.Count);
            Assert.AreEqual("1", invocationExpr.ArgumentList.Arguments[0].ToString());
            Assert.AreEqual("2", invocationExpr.ArgumentList.Arguments[1].ToString());
            Assert.AreEqual("(3, 2)", invocationExpr.ArgumentList.Arguments[2].ToString());

            var leftCompoundExp = (CompoundExpressionSyntax)compoundExp.Left;
            Assert.AreEqual(SyntaxKind.SimpleAssignmentExpression, leftCompoundExp.Left.Kind);
            Assert.AreEqual(SyntaxKind.SimpleAssignmentExpression, leftCompoundExp.Right.Kind);
        }

        private static ExpressionSyntax ParseExpression(string text)
        {
            var expression = SyntaxFactory.ParseExpression(text);

            Assert.AreEqual(0, expression.GetDiagnostics().Count());
            Assert.AreEqual(text, expression.ToString());

            return expression;
        }
    }
}