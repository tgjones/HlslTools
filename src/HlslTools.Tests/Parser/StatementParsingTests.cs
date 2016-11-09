using System.Linq;
using HlslTools.Syntax;
using NUnit.Framework;

namespace HlslTools.Tests.Parser
{
    [TestFixture]
    public class StatementParsingTests
    {
        [Test]
        public void TestName()
        {
            var text = "a();";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ExpressionStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var es = (ExpressionStatementSyntax)statement;
            Assert.NotNull(es.Expression);
            Assert.AreEqual(SyntaxKind.FunctionInvocationExpression, es.Expression.Kind);
            Assert.AreEqual("a()", es.Expression.ToString());
            Assert.NotNull(es.SemicolonToken);
            Assert.False(es.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestDottedName()
        {
            var text = "a.b();";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ExpressionStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var es = (ExpressionStatementSyntax)statement;
            Assert.NotNull(es.Expression);
            Assert.AreEqual(SyntaxKind.MethodInvocationExpression, es.Expression.Kind);
            Assert.AreEqual(SyntaxKind.IdentifierName, ((MethodInvocationExpressionSyntax)es.Expression).Target.Kind);
            Assert.AreEqual("a.b()", es.Expression.ToString());
            Assert.NotNull(es.SemicolonToken);
            Assert.False(es.SemicolonToken.IsMissing);
        }

        [TestCase(SyntaxKind.PlusPlusToken)]
        [TestCase(SyntaxKind.MinusMinusToken)]
        public void TestPostfixUnaryOperator(SyntaxKind kind)
        {
            var text = "a" + kind.GetText() + ";";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ExpressionStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var es = (ExpressionStatementSyntax)statement;
            Assert.NotNull(es.Expression);
            Assert.NotNull(es.SemicolonToken);
            Assert.False(es.SemicolonToken.IsMissing);

            var opKind = SyntaxFacts.GetPostfixUnaryExpression(kind);
            Assert.AreEqual(opKind, es.Expression.Kind);
            var us = (PostfixUnaryExpressionSyntax)es.Expression;
            Assert.AreEqual("a", us.Operand.ToString());
            Assert.AreEqual(kind, us.OperatorToken.Kind);
        }

        [Test]
        public void TestLocalDeclarationStatement()
        {
            var text = "T a;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.AreEqual("T", ds.Declaration.Type.ToString());
            Assert.AreEqual(1, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.AreEqual("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.Null(ds.Declaration.Variables[0].Initializer);

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestLocalDeclarationStatementWithArrayType()
        {
            var text = "T a[2];";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.AreEqual("T", ds.Declaration.Type.ToString());
            Assert.AreEqual(1, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.AreEqual("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.Null(ds.Declaration.Variables[0].Initializer);
            Assert.AreEqual(1, ds.Declaration.Variables[0].ArrayRankSpecifiers.Count);
            Assert.AreEqual("2", ds.Declaration.Variables[0].ArrayRankSpecifiers[0].Dimension.ToString());

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestLocalDeclarationStatementWithMultipleVariables()
        {
            var text = "T a, b, c;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.AreEqual("T", ds.Declaration.Type.ToString());
            Assert.AreEqual(3, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.AreEqual("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.Null(ds.Declaration.Variables[0].Initializer);

            Assert.NotNull(ds.Declaration.Variables[1].Identifier);
            Assert.AreEqual("b", ds.Declaration.Variables[1].Identifier.ToString());
            Assert.Null(ds.Declaration.Variables[1].Initializer);

            Assert.NotNull(ds.Declaration.Variables[2].Identifier);
            Assert.AreEqual("c", ds.Declaration.Variables[2].Identifier.ToString());
            Assert.Null(ds.Declaration.Variables[2].Initializer);

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestLocalDeclarationStatementWithInitializer()
        {
            var text = "T a = b;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.AreEqual("T", ds.Declaration.Type.ToString());
            Assert.AreEqual(1, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.AreEqual("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.NotNull(ds.Declaration.Variables[0].Initializer);
            Assert.AreEqual(ds.Declaration.Variables[0].Initializer.Kind, SyntaxKind.EqualsValueClause);

            var initializer = (EqualsValueClauseSyntax) ds.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.False(initializer.EqualsToken.IsMissing);
            Assert.NotNull(initializer.Value);
            Assert.AreEqual("b", initializer.Value.ToString());

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestLocalDeclarationStatementWithMultipleVariablesAndInitializers()
        {
            var text = "T a = va, b = vb, c = vc;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.AreEqual("T", ds.Declaration.Type.ToString());
            Assert.AreEqual(3, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.AreEqual("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.AreEqual(ds.Declaration.Variables[0].Initializer.Kind, SyntaxKind.EqualsValueClause);
            var initializer = (EqualsValueClauseSyntax) ds.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.False(initializer.EqualsToken.IsMissing);
            Assert.NotNull(initializer.Value);
            Assert.AreEqual("va", initializer.Value.ToString());

            Assert.NotNull(ds.Declaration.Variables[1].Identifier);
            Assert.AreEqual("b", ds.Declaration.Variables[1].Identifier.ToString());
            Assert.AreEqual(ds.Declaration.Variables[1].Initializer.Kind, SyntaxKind.EqualsValueClause);
            initializer = (EqualsValueClauseSyntax)ds.Declaration.Variables[1].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.False(initializer.EqualsToken.IsMissing);
            Assert.NotNull(initializer.Value);
            Assert.AreEqual("vb", initializer.Value.ToString());

            Assert.NotNull(ds.Declaration.Variables[2].Identifier);
            Assert.AreEqual("c", ds.Declaration.Variables[2].Identifier.ToString());
            Assert.AreEqual(ds.Declaration.Variables[2].Initializer.Kind, SyntaxKind.EqualsValueClause);
            initializer = (EqualsValueClauseSyntax)ds.Declaration.Variables[2].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.False(initializer.EqualsToken.IsMissing);
            Assert.NotNull(initializer.Value);
            Assert.AreEqual("vc", initializer.Value.ToString());

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestLocalDeclarationStatementWithArrayInitializer()
        {
            var text = "T a = {b, c};";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.AreEqual("T", ds.Declaration.Type.ToString());
            Assert.AreEqual(1, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.AreEqual("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.NotNull(ds.Declaration.Variables[0].Initializer);
            Assert.AreEqual(ds.Declaration.Variables[0].Initializer.Kind, SyntaxKind.EqualsValueClause);
            var initializer = (EqualsValueClauseSyntax)ds.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.False(initializer.EqualsToken.IsMissing);
            Assert.NotNull(initializer.Value);
            Assert.AreEqual(SyntaxKind.ArrayInitializerExpression, initializer.Value.Kind);
            Assert.AreEqual(2, ((ArrayInitializerExpressionSyntax) initializer.Value).Elements.Count);
            Assert.AreEqual("{b, c}", initializer.Value.ToString());

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestEmptyStatement()
        {
            var text = ";";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.EmptyStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var es = (EmptyStatementSyntax)statement;
            Assert.NotNull(es.SemicolonToken);
            Assert.False(es.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestBreakStatement()
        {
            var text = "break;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.BreakStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var b = (BreakStatementSyntax)statement;
            Assert.NotNull(b.BreakKeyword);
            Assert.False(b.BreakKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.BreakKeyword, b.BreakKeyword.Kind);
            Assert.NotNull(b.SemicolonToken);
            Assert.False(b.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestContinueStatement()
        {
            var text = "continue;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ContinueStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var cs = (ContinueStatementSyntax)statement;
            Assert.NotNull(cs.ContinueKeyword);
            Assert.False(cs.ContinueKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ContinueKeyword, cs.ContinueKeyword.Kind);
            Assert.NotNull(cs.SemicolonToken);
            Assert.False(cs.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestDiscardStatement()
        {
            var text = "discard;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.DiscardStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var cs = (DiscardStatementSyntax)statement;
            Assert.NotNull(cs.DiscardKeyword);
            Assert.False(cs.DiscardKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.DiscardKeyword, cs.DiscardKeyword.Kind);
            Assert.NotNull(cs.SemicolonToken);
            Assert.False(cs.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestReturn()
        {
            var text = "return;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ReturnStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var rs = (ReturnStatementSyntax)statement;
            Assert.NotNull(rs.ReturnKeyword);
            Assert.False(rs.ReturnKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ReturnKeyword, rs.ReturnKeyword.Kind);
            Assert.Null(rs.Expression);
            Assert.NotNull(rs.SemicolonToken);
            Assert.False(rs.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestReturnExpression()
        {
            var text = "return a;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ReturnStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var rs = (ReturnStatementSyntax)statement;
            Assert.NotNull(rs.ReturnKeyword);
            Assert.False(rs.ReturnKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ReturnKeyword, rs.ReturnKeyword.Kind);
            Assert.NotNull(rs.Expression);
            Assert.AreEqual("a", rs.Expression.ToString());
            Assert.NotNull(rs.SemicolonToken);
            Assert.False(rs.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestWhile()
        {
            var text = "while(a) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.WhileStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ws = (WhileStatementSyntax)statement;
            Assert.NotNull(ws.WhileKeyword);
            Assert.AreEqual(SyntaxKind.WhileKeyword, ws.WhileKeyword.Kind);
            Assert.NotNull(ws.OpenParenToken);
            Assert.NotNull(ws.Condition);
            Assert.NotNull(ws.CloseParenToken);
            Assert.AreEqual("a", ws.Condition.ToString());
            Assert.NotNull(ws.Statement);
            Assert.AreEqual(SyntaxKind.Block, ws.Statement.Kind);
        }

        [Test]
        public void TestDoWhile()
        {
            var text = "do { } while (a);";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.DoStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ds = (DoStatementSyntax)statement;
            Assert.NotNull(ds.DoKeyword);
            Assert.AreEqual(SyntaxKind.DoKeyword, ds.DoKeyword.Kind);
            Assert.NotNull(ds.Statement);
            Assert.NotNull(ds.WhileKeyword);
            Assert.AreEqual(SyntaxKind.WhileKeyword, ds.WhileKeyword.Kind);
            Assert.AreEqual(SyntaxKind.Block, ds.Statement.Kind);
            Assert.NotNull(ds.OpenParenToken);
            Assert.NotNull(ds.Condition);
            Assert.NotNull(ds.CloseParenToken);
            Assert.AreEqual("a", ds.Condition.ToString());
            Assert.NotNull(ds.SemicolonToken);
        }

        [Test]
        public void TestFor()
        {
            var text = "for(;;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ForStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);
            Assert.Null(fs.Declaration);
            Assert.IsNull(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.IsNull(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Test]
        public void TestForWithVariableDeclaration()
        {
            var text = "for(T a = 0;;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ForStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.NotNull(fs.Declaration);
            Assert.NotNull(fs.Declaration.Type);
            Assert.AreEqual("T", fs.Declaration.Type.ToString());
            Assert.AreEqual(1, fs.Declaration.Variables.Count);
            Assert.NotNull(fs.Declaration.Variables[0].Identifier);
            Assert.AreEqual("a", fs.Declaration.Variables[0].Identifier.ToString());
            Assert.NotNull(fs.Declaration.Variables[0].Initializer);
            var initializer = (EqualsValueClauseSyntax)fs.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.NotNull(initializer.Value);
            Assert.AreEqual("0", initializer.Value.ToString());

            Assert.IsNull(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.IsNull(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Test]
        public void TestForWithMultipleVariableDeclarations()
        {
            var text = "for(T a = 0, b = 1;;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ForStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.NotNull(fs.Declaration);
            Assert.NotNull(fs.Declaration.Type);
            Assert.AreEqual("T", fs.Declaration.Type.ToString());
            Assert.AreEqual(2, fs.Declaration.Variables.Count);

            Assert.NotNull(fs.Declaration.Variables[0].Identifier);
            Assert.AreEqual("a", fs.Declaration.Variables[0].Identifier.ToString());
            Assert.NotNull(fs.Declaration.Variables[0].Initializer);
            var initializer = (EqualsValueClauseSyntax) fs.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.NotNull(initializer.Value);
            Assert.AreEqual("0", initializer.Value.ToString());

            Assert.NotNull(fs.Declaration.Variables[1].Identifier);
            Assert.AreEqual("b", fs.Declaration.Variables[1].Identifier.ToString());
            Assert.NotNull(fs.Declaration.Variables[1].Initializer);
            initializer = (EqualsValueClauseSyntax)fs.Declaration.Variables[1].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.NotNull(initializer.Value);
            Assert.AreEqual("1", initializer.Value.ToString());

            Assert.IsNull(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.IsNull(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Test]
        public void TestForWithVariableInitializer()
        {
            var text = "for(a = 0;;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ForStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.Null(fs.Declaration);
            Assert.NotNull(fs.Initializer);
            Assert.AreEqual(SyntaxKind.SimpleAssignmentExpression, fs.Initializer.Kind);
            Assert.AreEqual("a = 0", fs.Initializer.ToString());

            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.IsNull(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Test]
        public void TestForWithMultipleVariableInitializers()
        {
            var text = "for(a = 0, b = 1;;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ForStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.Null(fs.Declaration);

            Assert.NotNull(fs.Initializer);
            Assert.AreEqual(SyntaxKind.CompoundExpression, fs.Initializer.Kind);
            var compExpr = (CompoundExpressionSyntax) fs.Initializer;
            Assert.AreEqual(SyntaxKind.SimpleAssignmentExpression, compExpr.Left.Kind);
            Assert.AreEqual("a = 0", compExpr.Left.ToString());
            Assert.AreEqual(SyntaxKind.SimpleAssignmentExpression, compExpr.Right.Kind);
            Assert.AreEqual("b = 1", compExpr.Right.ToString());

            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.IsNull(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Test]
        public void TestForWithCondition()
        {
            var text = "for(; a;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ForStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.Null(fs.Declaration);
            Assert.Null(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);

            Assert.NotNull(fs.Condition);
            Assert.AreEqual("a", fs.Condition.ToString());

            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.IsNull(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Test]
        public void TestForWithIncrementor()
        {
            var text = "for(; ; a++) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ForStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.Null(fs.Declaration);
            Assert.Null(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);

            Assert.NotNull(fs.Incrementor);
            Assert.AreEqual(SyntaxKind.PostIncrementExpression, fs.Incrementor.Kind);
            Assert.AreEqual("a++", fs.Incrementor.ToString());

            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Test]
        public void TestForWithMultipleIncrementors()
        {
            var text = "for(; ; a++, b++) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ForStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.Null(fs.Declaration);
            Assert.Null(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);

            Assert.NotNull(fs.Incrementor);
            Assert.AreEqual(SyntaxKind.CompoundExpression, fs.Incrementor.Kind);
            var compExpr = (CompoundExpressionSyntax) fs.Incrementor;
            Assert.AreEqual(SyntaxKind.PostIncrementExpression, compExpr.Left.Kind);
            Assert.AreEqual("a++", compExpr.Left.ToString());
            Assert.AreEqual(SyntaxKind.PostIncrementExpression, compExpr.Right.Kind);
            Assert.AreEqual("b++", compExpr.Right.ToString());

            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Test]
        public void TestForWithDeclarationConditionAndIncrementor()
        {
            var text = "for(T a = 0; a < 10; a++) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.ForStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.AreEqual(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.NotNull(fs.Declaration);
            Assert.NotNull(fs.Declaration.Type);
            Assert.AreEqual("T", fs.Declaration.Type.ToString());
            Assert.AreEqual(1, fs.Declaration.Variables.Count);
            Assert.NotNull(fs.Declaration.Variables[0].Identifier);
            Assert.AreEqual("a", fs.Declaration.Variables[0].Identifier.ToString());
            Assert.NotNull(fs.Declaration.Variables[0].Initializer);
            var initializer = (EqualsValueClauseSyntax)fs.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.NotNull(initializer.Value);
            Assert.AreEqual("0", initializer.Value.ToString());

            Assert.Null(fs.Initializer);

            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.NotNull(fs.Condition);
            Assert.AreEqual("a < 10", fs.Condition.ToString());

            Assert.NotNull(fs.SecondSemicolonToken);

            Assert.NotNull(fs.Incrementor);
            Assert.AreEqual(SyntaxKind.PostIncrementExpression, fs.Incrementor.Kind);
            Assert.AreEqual("a++", fs.Incrementor.ToString());

            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Test]
        public void TestIf()
        {
            var text = "if (a) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.IfStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ss = (IfStatementSyntax)statement;
            Assert.NotNull(ss.IfKeyword);
            Assert.AreEqual(SyntaxKind.IfKeyword, ss.IfKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Condition);
            Assert.AreEqual("a", ss.Condition.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.Statement);

            Assert.Null(ss.Else);
        }

        [Test]
        public void TestIfElse()
        {
            var text = "if (a) { } else { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.IfStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ss = (IfStatementSyntax)statement;
            Assert.NotNull(ss.IfKeyword);
            Assert.AreEqual(SyntaxKind.IfKeyword, ss.IfKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Condition);
            Assert.AreEqual("a", ss.Condition.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.Statement);

            Assert.NotNull(ss.Else);
            Assert.NotNull(ss.Else.ElseKeyword);
            Assert.AreEqual(SyntaxKind.ElseKeyword, ss.Else.ElseKeyword.Kind);
            Assert.NotNull(ss.Else.Statement);
        }

        [Test]
        public void TestSwitch()
        {
            var text = "switch (a) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.AreEqual(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.AreEqual("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);
            Assert.AreEqual(0, ss.Sections.Count);
            Assert.NotNull(ss.CloseBraceToken);
        }

        [Test]
        public void TestSwitchWithCase()
        {
            var text = "switch (a) { case b:; }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.AreEqual(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.AreEqual("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);

            Assert.AreEqual(1, ss.Sections.Count);
            Assert.AreEqual(1, ss.Sections[0].Labels.Count);
            Assert.NotNull(ss.Sections[0].Labels[0].Keyword);
            Assert.AreEqual(SyntaxKind.CaseKeyword, ss.Sections[0].Labels[0].Keyword.Kind);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax);
            Assert.NotNull(caseLabelSyntax.Value);
            Assert.AreEqual("b", caseLabelSyntax.Value.ToString());
            Assert.NotNull(caseLabelSyntax.ColonToken);
            Assert.AreEqual(1, ss.Sections[0].Statements.Count);
            Assert.AreEqual(";", ss.Sections[0].Statements[0].ToString());

            Assert.NotNull(ss.CloseBraceToken);
        }

        [Test]
        public void TestSwitchWithMultipleCases()
        {
            var text = "switch (a) { case b:; case c:; }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.AreEqual(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.AreEqual("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);

            Assert.AreEqual(2, ss.Sections.Count);

            Assert.AreEqual(1, ss.Sections[0].Labels.Count);
            Assert.NotNull(ss.Sections[0].Labels[0].Keyword);
            Assert.AreEqual(SyntaxKind.CaseKeyword, ss.Sections[0].Labels[0].Keyword.Kind);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax);
            Assert.NotNull(caseLabelSyntax.Value);
            Assert.AreEqual("b", caseLabelSyntax.Value.ToString());
            Assert.NotNull(caseLabelSyntax.ColonToken);
            Assert.AreEqual(1, ss.Sections[0].Statements.Count);
            Assert.AreEqual(";", ss.Sections[0].Statements[0].ToString());

            Assert.AreEqual(1, ss.Sections[1].Labels.Count);
            Assert.NotNull(ss.Sections[1].Labels[0].Keyword);
            Assert.AreEqual(SyntaxKind.CaseKeyword, ss.Sections[1].Labels[0].Keyword.Kind);
            var caseLabelSyntax2 = ss.Sections[1].Labels[0] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax2);
            Assert.NotNull(caseLabelSyntax2.Value);
            Assert.AreEqual("c", caseLabelSyntax2.Value.ToString());
            Assert.NotNull(caseLabelSyntax2.ColonToken);
            Assert.AreEqual(1, ss.Sections[1].Statements.Count);
            Assert.AreEqual(";", ss.Sections[0].Statements[0].ToString());

            Assert.NotNull(ss.CloseBraceToken);
        }

        [Test]
        public void TestSwitchWithDefaultCase()
        {
            var text = "switch (a) { default:; }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.AreEqual(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.AreEqual("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);

            Assert.AreEqual(1, ss.Sections.Count);

            Assert.AreEqual(1, ss.Sections[0].Labels.Count);
            Assert.NotNull(ss.Sections[0].Labels[0].Keyword);
            Assert.AreEqual(SyntaxKind.DefaultKeyword, ss.Sections[0].Labels[0].Keyword.Kind);
            Assert.AreEqual(SyntaxKind.DefaultSwitchLabel, ss.Sections[0].Labels[0].Kind);
            Assert.NotNull(ss.Sections[0].Labels[0].ColonToken);
            Assert.AreEqual(1, ss.Sections[0].Statements.Count);
            Assert.AreEqual(";", ss.Sections[0].Statements[0].ToString());

            Assert.NotNull(ss.CloseBraceToken);
        }

        [Test]
        public void TestSwitchWithMultipleLabelsOnOneCase()
        {
            var text = "switch (a) { case b: case c:; }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.AreEqual(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.AreEqual("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);

            Assert.AreEqual(1, ss.Sections.Count);

            Assert.AreEqual(2, ss.Sections[0].Labels.Count);
            Assert.NotNull(ss.Sections[0].Labels[0].Keyword);
            Assert.AreEqual(SyntaxKind.CaseKeyword, ss.Sections[0].Labels[0].Keyword.Kind);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax);
            Assert.NotNull(caseLabelSyntax.Value);
            Assert.AreEqual("b", caseLabelSyntax.Value.ToString());
            Assert.NotNull(ss.Sections[0].Labels[1].Keyword);
            Assert.AreEqual(SyntaxKind.CaseKeyword, ss.Sections[0].Labels[1].Keyword.Kind);
            var caseLabelSyntax2 = ss.Sections[0].Labels[1] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax2);
            Assert.NotNull(caseLabelSyntax2.Value);
            Assert.AreEqual("c", caseLabelSyntax2.Value.ToString());
            Assert.NotNull(ss.Sections[0].Labels[0].ColonToken);
            Assert.AreEqual(1, ss.Sections[0].Statements.Count);
            Assert.AreEqual(";", ss.Sections[0].Statements[0].ToString());

            Assert.NotNull(ss.CloseBraceToken);
        }

        [Test]
        public void TestSwitchWithMultipleStatementsOnOneCase()
        {
            var text = "switch (a) { case b: s1(); s2(); }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.AreEqual(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.AreEqual("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);

            Assert.AreEqual(1, ss.Sections.Count);

            Assert.AreEqual(1, ss.Sections[0].Labels.Count);
            Assert.NotNull(ss.Sections[0].Labels[0].Keyword);
            Assert.AreEqual(SyntaxKind.CaseKeyword, ss.Sections[0].Labels[0].Keyword.Kind);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax);
            Assert.NotNull(caseLabelSyntax.Value);
            Assert.AreEqual("b", caseLabelSyntax.Value.ToString());
            Assert.AreEqual(2, ss.Sections[0].Statements.Count);
            Assert.AreEqual("s1();", ss.Sections[0].Statements[0].ToString());
            Assert.AreEqual("s2();", ss.Sections[0].Statements[1].ToString());

            Assert.NotNull(ss.CloseBraceToken);
        }

        [Test]
        public void TestBlock()
        {
            var text = "{ a(); }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.Block, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var block = (BlockSyntax) statement;
            Assert.AreEqual(1, block.Statements.Count);
            var innerStatement = block.Statements[0];
            Assert.NotNull(innerStatement);
            Assert.AreEqual(SyntaxKind.ExpressionStatement, innerStatement.Kind);

            var es = (ExpressionStatementSyntax)innerStatement;
            Assert.NotNull(es.Expression);
            Assert.AreEqual(SyntaxKind.FunctionInvocationExpression, es.Expression.Kind);
            Assert.AreEqual("a()", es.Expression.ToString());
            Assert.NotNull(es.SemicolonToken);
            Assert.False(es.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestClassDeclarationStatement()
        {
            var text = "class C { int a; };";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.TypeDeclarationStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var typeDeclarationStatement = (TypeDeclarationStatementSyntax) statement;
            Assert.That(typeDeclarationStatement.Type.Kind, Is.EqualTo(SyntaxKind.ClassType));

            var cd = (ClassTypeSyntax)typeDeclarationStatement.Type;
            Assert.NotNull(cd.ClassKeyword);
            Assert.AreEqual("C", cd.Name.Text);
            Assert.AreEqual(SyntaxKind.ClassKeyword, cd.ClassKeyword.Kind);
            Assert.Null(cd.BaseList);
            Assert.NotNull(cd.OpenBraceToken);
            Assert.AreEqual(1, cd.Members.Count);
            Assert.AreEqual("int a;", cd.Members[0].ToString());
            Assert.NotNull(cd.CloseBraceToken);

            Assert.NotNull(typeDeclarationStatement.SemicolonToken);
        }

        [Test]
        public void TestClassDeclarationWithBaseStatement()
        {
            var text = "class C : B { int a; };";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.TypeDeclarationStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var typeDeclarationStatement = (TypeDeclarationStatementSyntax)statement;
            Assert.That(typeDeclarationStatement.Type.Kind, Is.EqualTo(SyntaxKind.ClassType));

            var cd = (ClassTypeSyntax)typeDeclarationStatement.Type;
            Assert.NotNull(cd.ClassKeyword);
            Assert.AreEqual("C", cd.Name.Text);
            Assert.AreEqual(SyntaxKind.ClassKeyword, cd.ClassKeyword.Kind);
            Assert.NotNull(cd.BaseList);
            Assert.NotNull(cd.BaseList.ColonToken);
            Assert.AreEqual("B", cd.BaseList.BaseType.ToString());
            Assert.NotNull(cd.OpenBraceToken);
            Assert.AreEqual(1, cd.Members.Count);
            Assert.AreEqual("int a;", cd.Members[0].ToString());
            Assert.NotNull(cd.CloseBraceToken);

            Assert.NotNull(typeDeclarationStatement.SemicolonToken);
        }

        [Test]
        public void TestInterfaceDeclarationStatement()
        {
            var text = "interface I { int foo(); };";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.TypeDeclarationStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var typeDeclarationStatement = (TypeDeclarationStatementSyntax)statement;
            Assert.That(typeDeclarationStatement.Type.Kind, Is.EqualTo(SyntaxKind.InterfaceType));

            var cd = (InterfaceTypeSyntax)typeDeclarationStatement.Type;
            Assert.NotNull(cd.InterfaceKeyword);
            Assert.AreEqual("I", cd.Name.Text);
            Assert.AreEqual(SyntaxKind.InterfaceKeyword, cd.InterfaceKeyword.Kind);
            Assert.NotNull(cd.OpenBraceToken);
            Assert.AreEqual(1, cd.Methods.Count);
            Assert.AreEqual("int foo();", cd.Methods[0].ToString());
            Assert.NotNull(cd.CloseBraceToken);

            Assert.NotNull(typeDeclarationStatement.SemicolonToken);
        }

        [Test]
        public void TestStructDeclarationStatement()
        {
            var text = "struct S { int a; };";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.TypeDeclarationStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var typeDeclarationStatement = (TypeDeclarationStatementSyntax)statement;
            Assert.That(typeDeclarationStatement.Type.Kind, Is.EqualTo(SyntaxKind.StructType));

            var cd = (StructTypeSyntax)typeDeclarationStatement.Type;
            Assert.NotNull(cd.StructKeyword);
            Assert.AreEqual("S", cd.Name.Text);
            Assert.AreEqual(SyntaxKind.StructKeyword, cd.StructKeyword.Kind);
            Assert.NotNull(cd.OpenBraceToken);
            Assert.AreEqual(1, cd.Fields.Count);
            Assert.AreEqual("int a;", cd.Fields[0].ToString());
            Assert.NotNull(cd.CloseBraceToken);

            Assert.NotNull(typeDeclarationStatement.SemicolonToken);
        }

        [Test]
        public void TestTypedefStatement()
        {
            var text = "typedef float2 Point, Vector;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.AreEqual(SyntaxKind.TypedefStatement, statement.Kind);
            Assert.AreEqual(text, statement.ToString());
            Assert.AreEqual(0, statement.GetDiagnostics().Count());

            var typedefStatement = (TypedefStatementSyntax) statement;
            Assert.That(typedefStatement.Type.Kind, Is.EqualTo(SyntaxKind.PredefinedVectorType));

            Assert.AreEqual(2, typedefStatement.Declarators.Count);

            Assert.AreEqual("Point", typedefStatement.Declarators[0].Identifier.Text);
            Assert.AreEqual("Vector", typedefStatement.Declarators[1].Identifier.Text);

            Assert.NotNull(typedefStatement.SemicolonToken);
        }

        private static StatementSyntax ParseStatement(string text)
        {
            return SyntaxFactory.ParseStatement(text);
        }
    }
}