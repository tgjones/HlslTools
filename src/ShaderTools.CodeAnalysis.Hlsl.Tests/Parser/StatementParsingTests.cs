using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Parser
{
    public class StatementParsingTests
    {
        [Fact]
        public void TestName()
        {
            var text = "a();";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ExpressionStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var es = (ExpressionStatementSyntax)statement;
            Assert.NotNull(es.Expression);
            Assert.Equal(SyntaxKind.FunctionInvocationExpression, es.Expression.Kind);
            Assert.Equal("a()", es.Expression.ToString());
            Assert.NotNull(es.SemicolonToken);
            Assert.False(es.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestDottedName()
        {
            var text = "a.b();";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ExpressionStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var es = (ExpressionStatementSyntax)statement;
            Assert.NotNull(es.Expression);
            Assert.Equal(SyntaxKind.MethodInvocationExpression, es.Expression.Kind);
            Assert.Equal(SyntaxKind.IdentifierName, ((MethodInvocationExpressionSyntax)es.Expression).Target.Kind);
            Assert.Equal("a.b()", es.Expression.ToString());
            Assert.NotNull(es.SemicolonToken);
            Assert.False(es.SemicolonToken.IsMissing);
        }

        [InlineData(SyntaxKind.PlusPlusToken)]
        [InlineData(SyntaxKind.MinusMinusToken)]
        public void TestPostfixUnaryOperator(SyntaxKind kind)
        {
            var text = "a" + kind.GetText() + ";";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ExpressionStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var es = (ExpressionStatementSyntax)statement;
            Assert.NotNull(es.Expression);
            Assert.NotNull(es.SemicolonToken);
            Assert.False(es.SemicolonToken.IsMissing);

            var opKind = SyntaxFacts.GetPostfixUnaryExpression(kind);
            Assert.Equal(opKind, es.Expression.Kind);
            var us = (PostfixUnaryExpressionSyntax)es.Expression;
            Assert.Equal("a", us.Operand.ToString());
            Assert.Equal(kind, us.OperatorToken.Kind);
        }

        [Fact]
        public void TestLocalDeclarationStatement()
        {
            var text = "T a;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.Equal("T", ds.Declaration.Type.ToString());
            Assert.Equal(1, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.Equal("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.Null(ds.Declaration.Variables[0].Initializer);

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestLocalDeclarationStatementWithArrayType()
        {
            var text = "T a[2];";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.Equal("T", ds.Declaration.Type.ToString());
            Assert.Equal(1, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.Equal("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.Null(ds.Declaration.Variables[0].Initializer);
            Assert.Equal(1, ds.Declaration.Variables[0].ArrayRankSpecifiers.Count);
            Assert.Equal("2", ds.Declaration.Variables[0].ArrayRankSpecifiers[0].Dimension.ToString());

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestLocalDeclarationStatementWithMultipleVariables()
        {
            var text = "T a, b, c;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.Equal("T", ds.Declaration.Type.ToString());
            Assert.Equal(3, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.Equal("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.Null(ds.Declaration.Variables[0].Initializer);

            Assert.NotNull(ds.Declaration.Variables[1].Identifier);
            Assert.Equal("b", ds.Declaration.Variables[1].Identifier.ToString());
            Assert.Null(ds.Declaration.Variables[1].Initializer);

            Assert.NotNull(ds.Declaration.Variables[2].Identifier);
            Assert.Equal("c", ds.Declaration.Variables[2].Identifier.ToString());
            Assert.Null(ds.Declaration.Variables[2].Initializer);

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestLocalDeclarationStatementWithInitializer()
        {
            var text = "T a = b;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.Equal("T", ds.Declaration.Type.ToString());
            Assert.Equal(1, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.Equal("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.NotNull(ds.Declaration.Variables[0].Initializer);
            Assert.Equal(ds.Declaration.Variables[0].Initializer.Kind, SyntaxKind.EqualsValueClause);

            var initializer = (EqualsValueClauseSyntax) ds.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.False(initializer.EqualsToken.IsMissing);
            Assert.NotNull(initializer.Value);
            Assert.Equal("b", initializer.Value.ToString());

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestLocalDeclarationStatementWithMultipleVariablesAndInitializers()
        {
            var text = "T a = va, b = vb, c = vc;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.Equal("T", ds.Declaration.Type.ToString());
            Assert.Equal(3, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.Equal("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.Equal(ds.Declaration.Variables[0].Initializer.Kind, SyntaxKind.EqualsValueClause);
            var initializer = (EqualsValueClauseSyntax) ds.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.False(initializer.EqualsToken.IsMissing);
            Assert.NotNull(initializer.Value);
            Assert.Equal("va", initializer.Value.ToString());

            Assert.NotNull(ds.Declaration.Variables[1].Identifier);
            Assert.Equal("b", ds.Declaration.Variables[1].Identifier.ToString());
            Assert.Equal(ds.Declaration.Variables[1].Initializer.Kind, SyntaxKind.EqualsValueClause);
            initializer = (EqualsValueClauseSyntax)ds.Declaration.Variables[1].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.False(initializer.EqualsToken.IsMissing);
            Assert.NotNull(initializer.Value);
            Assert.Equal("vb", initializer.Value.ToString());

            Assert.NotNull(ds.Declaration.Variables[2].Identifier);
            Assert.Equal("c", ds.Declaration.Variables[2].Identifier.ToString());
            Assert.Equal(ds.Declaration.Variables[2].Initializer.Kind, SyntaxKind.EqualsValueClause);
            initializer = (EqualsValueClauseSyntax)ds.Declaration.Variables[2].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.False(initializer.EqualsToken.IsMissing);
            Assert.NotNull(initializer.Value);
            Assert.Equal("vc", initializer.Value.ToString());

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestLocalDeclarationStatementWithArrayInitializer()
        {
            var text = "T a = {b, c};";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ds = (VariableDeclarationStatementSyntax)statement;
            Assert.NotNull(ds.Declaration.Type);
            Assert.Equal("T", ds.Declaration.Type.ToString());
            Assert.Equal(1, ds.Declaration.Variables.Count);

            Assert.NotNull(ds.Declaration.Variables[0].Identifier);
            Assert.Equal("a", ds.Declaration.Variables[0].Identifier.ToString());
            Assert.NotNull(ds.Declaration.Variables[0].Initializer);
            Assert.Equal(ds.Declaration.Variables[0].Initializer.Kind, SyntaxKind.EqualsValueClause);
            var initializer = (EqualsValueClauseSyntax)ds.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.False(initializer.EqualsToken.IsMissing);
            Assert.NotNull(initializer.Value);
            Assert.Equal(SyntaxKind.ArrayInitializerExpression, initializer.Value.Kind);
            Assert.Equal(2, ((ArrayInitializerExpressionSyntax) initializer.Value).Elements.Count);
            Assert.Equal("{b, c}", initializer.Value.ToString());

            Assert.NotNull(ds.SemicolonToken);
            Assert.False(ds.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestEmptyStatement()
        {
            var text = ";";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.EmptyStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var es = (EmptyStatementSyntax)statement;
            Assert.NotNull(es.SemicolonToken);
            Assert.False(es.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestBreakStatement()
        {
            var text = "break;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.BreakStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var b = (BreakStatementSyntax)statement;
            Assert.NotNull(b.BreakKeyword);
            Assert.False(b.BreakKeyword.IsMissing);
            Assert.Equal(SyntaxKind.BreakKeyword, b.BreakKeyword.Kind);
            Assert.NotNull(b.SemicolonToken);
            Assert.False(b.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestContinueStatement()
        {
            var text = "continue;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ContinueStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var cs = (ContinueStatementSyntax)statement;
            Assert.NotNull(cs.ContinueKeyword);
            Assert.False(cs.ContinueKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ContinueKeyword, cs.ContinueKeyword.Kind);
            Assert.NotNull(cs.SemicolonToken);
            Assert.False(cs.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestDiscardStatement()
        {
            var text = "discard;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.DiscardStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var cs = (DiscardStatementSyntax)statement;
            Assert.NotNull(cs.DiscardKeyword);
            Assert.False(cs.DiscardKeyword.IsMissing);
            Assert.Equal(SyntaxKind.DiscardKeyword, cs.DiscardKeyword.Kind);
            Assert.NotNull(cs.SemicolonToken);
            Assert.False(cs.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestReturn()
        {
            var text = "return;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ReturnStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var rs = (ReturnStatementSyntax)statement;
            Assert.NotNull(rs.ReturnKeyword);
            Assert.False(rs.ReturnKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ReturnKeyword, rs.ReturnKeyword.Kind);
            Assert.Null(rs.Expression);
            Assert.NotNull(rs.SemicolonToken);
            Assert.False(rs.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestReturnExpression()
        {
            var text = "return a;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ReturnStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var rs = (ReturnStatementSyntax)statement;
            Assert.NotNull(rs.ReturnKeyword);
            Assert.False(rs.ReturnKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ReturnKeyword, rs.ReturnKeyword.Kind);
            Assert.NotNull(rs.Expression);
            Assert.Equal("a", rs.Expression.ToString());
            Assert.NotNull(rs.SemicolonToken);
            Assert.False(rs.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestWhile()
        {
            var text = "while(a) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.WhileStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ws = (WhileStatementSyntax)statement;
            Assert.NotNull(ws.WhileKeyword);
            Assert.Equal(SyntaxKind.WhileKeyword, ws.WhileKeyword.Kind);
            Assert.NotNull(ws.OpenParenToken);
            Assert.NotNull(ws.Condition);
            Assert.NotNull(ws.CloseParenToken);
            Assert.Equal("a", ws.Condition.ToString());
            Assert.NotNull(ws.Statement);
            Assert.Equal(SyntaxKind.Block, ws.Statement.Kind);
        }

        [Fact]
        public void TestDoWhile()
        {
            var text = "do { } while (a);";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.DoStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ds = (DoStatementSyntax)statement;
            Assert.NotNull(ds.DoKeyword);
            Assert.Equal(SyntaxKind.DoKeyword, ds.DoKeyword.Kind);
            Assert.NotNull(ds.Statement);
            Assert.NotNull(ds.WhileKeyword);
            Assert.Equal(SyntaxKind.WhileKeyword, ds.WhileKeyword.Kind);
            Assert.Equal(SyntaxKind.Block, ds.Statement.Kind);
            Assert.NotNull(ds.OpenParenToken);
            Assert.NotNull(ds.Condition);
            Assert.NotNull(ds.CloseParenToken);
            Assert.Equal("a", ds.Condition.ToString());
            Assert.NotNull(ds.SemicolonToken);
        }

        [Fact]
        public void TestFor()
        {
            var text = "[unroll] for(;;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ForStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);
            Assert.Null(fs.Declaration);
            Assert.Null(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.Null(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Fact]
        public void TestForWithVariableDeclaration()
        {
            var text = "for(T a = 0;;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ForStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.NotNull(fs.Declaration);
            Assert.NotNull(fs.Declaration.Type);
            Assert.Equal("T", fs.Declaration.Type.ToString());
            Assert.Equal(1, fs.Declaration.Variables.Count);
            Assert.NotNull(fs.Declaration.Variables[0].Identifier);
            Assert.Equal("a", fs.Declaration.Variables[0].Identifier.ToString());
            Assert.NotNull(fs.Declaration.Variables[0].Initializer);
            var initializer = (EqualsValueClauseSyntax)fs.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.NotNull(initializer.Value);
            Assert.Equal("0", initializer.Value.ToString());

            Assert.Null(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.Null(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Fact]
        public void TestForWithMultipleVariableDeclarations()
        {
            var text = "for(T a = 0, b = 1;;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ForStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.NotNull(fs.Declaration);
            Assert.NotNull(fs.Declaration.Type);
            Assert.Equal("T", fs.Declaration.Type.ToString());
            Assert.Equal(2, fs.Declaration.Variables.Count);

            Assert.NotNull(fs.Declaration.Variables[0].Identifier);
            Assert.Equal("a", fs.Declaration.Variables[0].Identifier.ToString());
            Assert.NotNull(fs.Declaration.Variables[0].Initializer);
            var initializer = (EqualsValueClauseSyntax) fs.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.NotNull(initializer.Value);
            Assert.Equal("0", initializer.Value.ToString());

            Assert.NotNull(fs.Declaration.Variables[1].Identifier);
            Assert.Equal("b", fs.Declaration.Variables[1].Identifier.ToString());
            Assert.NotNull(fs.Declaration.Variables[1].Initializer);
            initializer = (EqualsValueClauseSyntax)fs.Declaration.Variables[1].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.NotNull(initializer.Value);
            Assert.Equal("1", initializer.Value.ToString());

            Assert.Null(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.Null(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Fact]
        public void TestForWithVariableInitializer()
        {
            var text = "for(a = 0;;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ForStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.Null(fs.Declaration);
            Assert.NotNull(fs.Initializer);
            Assert.Equal(SyntaxKind.SimpleAssignmentExpression, fs.Initializer.Kind);
            Assert.Equal("a = 0", fs.Initializer.ToString());

            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.Null(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Fact]
        public void TestForWithMultipleVariableInitializers()
        {
            var text = "for(a = 0, b = 1;;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ForStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.Null(fs.Declaration);

            Assert.NotNull(fs.Initializer);
            Assert.Equal(SyntaxKind.CompoundExpression, fs.Initializer.Kind);
            var compExpr = (CompoundExpressionSyntax) fs.Initializer;
            Assert.Equal(SyntaxKind.SimpleAssignmentExpression, compExpr.Left.Kind);
            Assert.Equal("a = 0", compExpr.Left.ToString());
            Assert.Equal(SyntaxKind.SimpleAssignmentExpression, compExpr.Right.Kind);
            Assert.Equal("b = 1", compExpr.Right.ToString());

            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.Null(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Fact]
        public void TestForWithCondition()
        {
            var text = "for(; a;) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ForStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.Null(fs.Declaration);
            Assert.Null(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);

            Assert.NotNull(fs.Condition);
            Assert.Equal("a", fs.Condition.ToString());

            Assert.NotNull(fs.SecondSemicolonToken);
            Assert.Null(fs.Incrementor);
            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Fact]
        public void TestForWithIncrementor()
        {
            var text = "for(; ; a++) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ForStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.Null(fs.Declaration);
            Assert.Null(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);

            Assert.NotNull(fs.Incrementor);
            Assert.Equal(SyntaxKind.PostIncrementExpression, fs.Incrementor.Kind);
            Assert.Equal("a++", fs.Incrementor.ToString());

            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Fact]
        public void TestForWithMultipleIncrementors()
        {
            var text = "for(; ; a++, b++) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ForStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.Null(fs.Declaration);
            Assert.Null(fs.Initializer);
            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.Null(fs.Condition);
            Assert.NotNull(fs.SecondSemicolonToken);

            Assert.NotNull(fs.Incrementor);
            Assert.Equal(SyntaxKind.CompoundExpression, fs.Incrementor.Kind);
            var compExpr = (CompoundExpressionSyntax) fs.Incrementor;
            Assert.Equal(SyntaxKind.PostIncrementExpression, compExpr.Left.Kind);
            Assert.Equal("a++", compExpr.Left.ToString());
            Assert.Equal(SyntaxKind.PostIncrementExpression, compExpr.Right.Kind);
            Assert.Equal("b++", compExpr.Right.ToString());

            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Fact]
        public void TestForWithDeclarationConditionAndIncrementor()
        {
            var text = "for(T a = 0; a < 10; a++) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.ForStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var fs = (ForStatementSyntax)statement;
            Assert.NotNull(fs.ForKeyword);
            Assert.False(fs.ForKeyword.IsMissing);
            Assert.Equal(SyntaxKind.ForKeyword, fs.ForKeyword.Kind);
            Assert.NotNull(fs.OpenParenToken);

            Assert.NotNull(fs.Declaration);
            Assert.NotNull(fs.Declaration.Type);
            Assert.Equal("T", fs.Declaration.Type.ToString());
            Assert.Equal(1, fs.Declaration.Variables.Count);
            Assert.NotNull(fs.Declaration.Variables[0].Identifier);
            Assert.Equal("a", fs.Declaration.Variables[0].Identifier.ToString());
            Assert.NotNull(fs.Declaration.Variables[0].Initializer);
            var initializer = (EqualsValueClauseSyntax)fs.Declaration.Variables[0].Initializer;
            Assert.NotNull(initializer.EqualsToken);
            Assert.NotNull(initializer.Value);
            Assert.Equal("0", initializer.Value.ToString());

            Assert.Null(fs.Initializer);

            Assert.NotNull(fs.FirstSemicolonToken);
            Assert.NotNull(fs.Condition);
            Assert.Equal("a < 10", fs.Condition.ToString());

            Assert.NotNull(fs.SecondSemicolonToken);

            Assert.NotNull(fs.Incrementor);
            Assert.Equal(SyntaxKind.PostIncrementExpression, fs.Incrementor.Kind);
            Assert.Equal("a++", fs.Incrementor.ToString());

            Assert.NotNull(fs.CloseParenToken);
            Assert.NotNull(fs.Statement);
        }

        [Fact]
        public void TestIf()
        {
            var text = "if (a) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.IfStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ss = (IfStatementSyntax)statement;
            Assert.NotNull(ss.IfKeyword);
            Assert.Equal(SyntaxKind.IfKeyword, ss.IfKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Condition);
            Assert.Equal("a", ss.Condition.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.Statement);

            Assert.Null(ss.Else);
        }

        [Fact]
        public void TestIfElse()
        {
            var text = "if (a) { } else { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.IfStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ss = (IfStatementSyntax)statement;
            Assert.NotNull(ss.IfKeyword);
            Assert.Equal(SyntaxKind.IfKeyword, ss.IfKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Condition);
            Assert.Equal("a", ss.Condition.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.Statement);

            Assert.NotNull(ss.Else);
            Assert.NotNull(ss.Else.ElseKeyword);
            Assert.Equal(SyntaxKind.ElseKeyword, ss.Else.ElseKeyword.Kind);
            Assert.NotNull(ss.Else.Statement);
        }

        [Fact]
        public void TestSwitch()
        {
            var text = "switch (a) { }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.Equal(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.Equal("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);
            Assert.Equal(0, ss.Sections.Count);
            Assert.NotNull(ss.CloseBraceToken);
        }

        [Fact]
        public void TestSwitchWithCase()
        {
            var text = "switch (a) { case b:; }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.Equal(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.Equal("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);

            Assert.Equal(1, ss.Sections.Count);
            Assert.Equal(1, ss.Sections[0].Labels.Count);
            Assert.NotNull(ss.Sections[0].Labels[0].Keyword);
            Assert.Equal(SyntaxKind.CaseKeyword, ss.Sections[0].Labels[0].Keyword.Kind);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax);
            Assert.NotNull(caseLabelSyntax.Value);
            Assert.Equal("b", caseLabelSyntax.Value.ToString());
            Assert.NotNull(caseLabelSyntax.ColonToken);
            Assert.Equal(1, ss.Sections[0].Statements.Count);
            Assert.Equal(";", ss.Sections[0].Statements[0].ToString());

            Assert.NotNull(ss.CloseBraceToken);
        }

        [Fact]
        public void TestSwitchWithMultipleCases()
        {
            var text = "switch (a) { case b:; case c:; }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.Equal(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.Equal("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);

            Assert.Equal(2, ss.Sections.Count);

            Assert.Equal(1, ss.Sections[0].Labels.Count);
            Assert.NotNull(ss.Sections[0].Labels[0].Keyword);
            Assert.Equal(SyntaxKind.CaseKeyword, ss.Sections[0].Labels[0].Keyword.Kind);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax);
            Assert.NotNull(caseLabelSyntax.Value);
            Assert.Equal("b", caseLabelSyntax.Value.ToString());
            Assert.NotNull(caseLabelSyntax.ColonToken);
            Assert.Equal(1, ss.Sections[0].Statements.Count);
            Assert.Equal(";", ss.Sections[0].Statements[0].ToString());

            Assert.Equal(1, ss.Sections[1].Labels.Count);
            Assert.NotNull(ss.Sections[1].Labels[0].Keyword);
            Assert.Equal(SyntaxKind.CaseKeyword, ss.Sections[1].Labels[0].Keyword.Kind);
            var caseLabelSyntax2 = ss.Sections[1].Labels[0] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax2);
            Assert.NotNull(caseLabelSyntax2.Value);
            Assert.Equal("c", caseLabelSyntax2.Value.ToString());
            Assert.NotNull(caseLabelSyntax2.ColonToken);
            Assert.Equal(1, ss.Sections[1].Statements.Count);
            Assert.Equal(";", ss.Sections[0].Statements[0].ToString());

            Assert.NotNull(ss.CloseBraceToken);
        }

        [Fact]
        public void TestSwitchWithDefaultCase()
        {
            var text = "switch (a) { default:; }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.Equal(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.Equal("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);

            Assert.Equal(1, ss.Sections.Count);

            Assert.Equal(1, ss.Sections[0].Labels.Count);
            Assert.NotNull(ss.Sections[0].Labels[0].Keyword);
            Assert.Equal(SyntaxKind.DefaultKeyword, ss.Sections[0].Labels[0].Keyword.Kind);
            Assert.Equal(SyntaxKind.DefaultSwitchLabel, ss.Sections[0].Labels[0].Kind);
            Assert.NotNull(ss.Sections[0].Labels[0].ColonToken);
            Assert.Equal(1, ss.Sections[0].Statements.Count);
            Assert.Equal(";", ss.Sections[0].Statements[0].ToString());

            Assert.NotNull(ss.CloseBraceToken);
        }

        [Fact]
        public void TestSwitchWithMultipleLabelsOnOneCase()
        {
            var text = "switch (a) { case b: case c:; }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.Equal(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.Equal("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);

            Assert.Equal(1, ss.Sections.Count);

            Assert.Equal(2, ss.Sections[0].Labels.Count);
            Assert.NotNull(ss.Sections[0].Labels[0].Keyword);
            Assert.Equal(SyntaxKind.CaseKeyword, ss.Sections[0].Labels[0].Keyword.Kind);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax);
            Assert.NotNull(caseLabelSyntax.Value);
            Assert.Equal("b", caseLabelSyntax.Value.ToString());
            Assert.NotNull(ss.Sections[0].Labels[1].Keyword);
            Assert.Equal(SyntaxKind.CaseKeyword, ss.Sections[0].Labels[1].Keyword.Kind);
            var caseLabelSyntax2 = ss.Sections[0].Labels[1] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax2);
            Assert.NotNull(caseLabelSyntax2.Value);
            Assert.Equal("c", caseLabelSyntax2.Value.ToString());
            Assert.NotNull(ss.Sections[0].Labels[0].ColonToken);
            Assert.Equal(1, ss.Sections[0].Statements.Count);
            Assert.Equal(";", ss.Sections[0].Statements[0].ToString());

            Assert.NotNull(ss.CloseBraceToken);
        }

        [Fact]
        public void TestSwitchWithMultipleStatementsOnOneCase()
        {
            var text = "switch (a) { case b: s1(); s2(); }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.SwitchStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var ss = (SwitchStatementSyntax)statement;
            Assert.NotNull(ss.SwitchKeyword);
            Assert.Equal(SyntaxKind.SwitchKeyword, ss.SwitchKeyword.Kind);
            Assert.NotNull(ss.OpenParenToken);
            Assert.NotNull(ss.Expression);
            Assert.Equal("a", ss.Expression.ToString());
            Assert.NotNull(ss.CloseParenToken);
            Assert.NotNull(ss.OpenBraceToken);

            Assert.Equal(1, ss.Sections.Count);

            Assert.Equal(1, ss.Sections[0].Labels.Count);
            Assert.NotNull(ss.Sections[0].Labels[0].Keyword);
            Assert.Equal(SyntaxKind.CaseKeyword, ss.Sections[0].Labels[0].Keyword.Kind);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            Assert.NotNull(caseLabelSyntax);
            Assert.NotNull(caseLabelSyntax.Value);
            Assert.Equal("b", caseLabelSyntax.Value.ToString());
            Assert.Equal(2, ss.Sections[0].Statements.Count);
            Assert.Equal("s1();", ss.Sections[0].Statements[0].ToString());
            Assert.Equal("s2();", ss.Sections[0].Statements[1].ToString());

            Assert.NotNull(ss.CloseBraceToken);
        }

        [Fact]
        public void TestBlock()
        {
            var text = "{ a(); }";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.Block, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var block = (BlockSyntax) statement;
            Assert.Equal(1, block.Statements.Count);
            var innerStatement = block.Statements[0];
            Assert.NotNull(innerStatement);
            Assert.Equal(SyntaxKind.ExpressionStatement, innerStatement.Kind);

            var es = (ExpressionStatementSyntax)innerStatement;
            Assert.NotNull(es.Expression);
            Assert.Equal(SyntaxKind.FunctionInvocationExpression, es.Expression.Kind);
            Assert.Equal("a()", es.Expression.ToString());
            Assert.NotNull(es.SemicolonToken);
            Assert.False(es.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestClassDeclarationStatement()
        {
            var text = "class C { int a; };";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.TypeDeclarationStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var typeDeclarationStatement = (TypeDeclarationStatementSyntax) statement;
            Assert.Equal(SyntaxKind.ClassType, typeDeclarationStatement.Type.Kind);

            var cd = (StructTypeSyntax)typeDeclarationStatement.Type;
            Assert.NotNull(cd.StructKeyword);
            Assert.Equal("C", cd.Name.Text);
            Assert.Equal(SyntaxKind.ClassKeyword, cd.StructKeyword.Kind);
            Assert.Null(cd.BaseList);
            Assert.NotNull(cd.OpenBraceToken);
            Assert.Equal(1, cd.Members.Count);
            Assert.Equal("int a;", cd.Members[0].ToString());
            Assert.NotNull(cd.CloseBraceToken);

            Assert.NotNull(typeDeclarationStatement.SemicolonToken);
        }

        [Fact]
        public void TestClassDeclarationWithBaseStatement()
        {
            var text = "class C : B { int a; };";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.TypeDeclarationStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var typeDeclarationStatement = (TypeDeclarationStatementSyntax)statement;
            Assert.Equal(SyntaxKind.ClassType, typeDeclarationStatement.Type.Kind);

            var cd = (StructTypeSyntax)typeDeclarationStatement.Type;
            Assert.NotNull(cd.StructKeyword);
            Assert.Equal("C", cd.Name.Text);
            Assert.Equal(SyntaxKind.ClassKeyword, cd.StructKeyword.Kind);
            Assert.NotNull(cd.BaseList);
            Assert.NotNull(cd.BaseList.ColonToken);
            Assert.Equal("B", cd.BaseList.BaseType.ToString());
            Assert.NotNull(cd.OpenBraceToken);
            Assert.Equal(1, cd.Members.Count);
            Assert.Equal("int a;", cd.Members[0].ToString());
            Assert.NotNull(cd.CloseBraceToken);

            Assert.NotNull(typeDeclarationStatement.SemicolonToken);
        }

        [Fact]
        public void TestInterfaceDeclarationStatement()
        {
            var text = "interface I { int foo(); };";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.TypeDeclarationStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var typeDeclarationStatement = (TypeDeclarationStatementSyntax)statement;
            Assert.Equal(SyntaxKind.InterfaceType, typeDeclarationStatement.Type.Kind);

            var cd = (InterfaceTypeSyntax)typeDeclarationStatement.Type;
            Assert.NotNull(cd.InterfaceKeyword);
            Assert.Equal("I", cd.Name.Text);
            Assert.Equal(SyntaxKind.InterfaceKeyword, cd.InterfaceKeyword.Kind);
            Assert.NotNull(cd.OpenBraceToken);
            Assert.Equal(1, cd.Methods.Count);
            Assert.Equal("int foo();", cd.Methods[0].ToString());
            Assert.NotNull(cd.CloseBraceToken);

            Assert.NotNull(typeDeclarationStatement.SemicolonToken);
        }

        [Fact]
        public void TestStructDeclarationStatement()
        {
            var text = "struct S { int a; };";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.TypeDeclarationStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var typeDeclarationStatement = (TypeDeclarationStatementSyntax)statement;
            Assert.Equal(SyntaxKind.StructType, typeDeclarationStatement.Type.Kind);

            var cd = (StructTypeSyntax)typeDeclarationStatement.Type;
            Assert.NotNull(cd.StructKeyword);
            Assert.Equal("S", cd.Name.Text);
            Assert.Equal(SyntaxKind.StructKeyword, cd.StructKeyword.Kind);
            Assert.NotNull(cd.OpenBraceToken);
            Assert.Equal(1, cd.Members.Count);
            Assert.Equal("int a;", cd.Members[0].ToString());
            Assert.NotNull(cd.CloseBraceToken);

            Assert.NotNull(typeDeclarationStatement.SemicolonToken);
        }

        [Fact]
        public void TestTypedefStatement()
        {
            var text = "typedef float2 Point, Vector;";
            var statement = ParseStatement(text);

            Assert.NotNull(statement);
            Assert.Equal(SyntaxKind.TypedefStatement, statement.Kind);
            Assert.Equal(text, statement.ToString());
            Assert.Equal(0, statement.GetDiagnostics().Count());

            var typedefStatement = (TypedefStatementSyntax) statement;
            Assert.Equal(SyntaxKind.PredefinedVectorType, typedefStatement.Type.Kind);

            Assert.Equal(2, typedefStatement.Declarators.Count);

            Assert.Equal("Point", typedefStatement.Declarators[0].Identifier.Text);
            Assert.Equal("Vector", typedefStatement.Declarators[1].Identifier.Text);

            Assert.NotNull(typedefStatement.SemicolonToken);
        }

        private static StatementSyntax ParseStatement(string text)
        {
            return SyntaxFactory.ParseStatement(text);
        }
    }
}