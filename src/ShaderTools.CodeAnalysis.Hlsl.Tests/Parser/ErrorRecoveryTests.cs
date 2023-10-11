using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Parser
{
    public class ErrorRecoveryTests
    {
        [Fact]
        public void HandlesSingleMissingToken()
        {
            var ast = BuildSyntaxTree("struct s int a; };");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            Assert.IsType<TypeDeclarationStatementSyntax>(ast.ChildNodes[0]);

            var typeDeclarationSyntax = (TypeDeclarationStatementSyntax) ast.ChildNodes[0];
            Assert.Equal(SyntaxKind.StructType, typeDeclarationSyntax.Type.Kind);

            var structDefinitionSyntax = (StructTypeSyntax)typeDeclarationSyntax.Type;
            Assert.False(structDefinitionSyntax.StructKeyword.IsMissing);
            Assert.False(structDefinitionSyntax.Name.IsMissing);
            Assert.True(structDefinitionSyntax.OpenBraceToken.IsMissing);
            Assert.False(structDefinitionSyntax.CloseBraceToken.IsMissing);

            Assert.False(typeDeclarationSyntax.SemicolonToken.IsMissing);
        }

        [Fact]
        public void HandlesSingleExtraToken()
        {
            var ast = BuildSyntaxTree("struct s t { int a; };");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            Assert.IsType<TypeDeclarationStatementSyntax>(ast.ChildNodes[0]);

            var typeDeclarationSyntax = (TypeDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.Equal(SyntaxKind.StructType, typeDeclarationSyntax.Type.Kind);

            var structDefinitionSyntax = (StructTypeSyntax)typeDeclarationSyntax.Type;
            Assert.False(structDefinitionSyntax.StructKeyword.IsMissing);
            Assert.False(structDefinitionSyntax.Name.IsMissing);
            Assert.False(structDefinitionSyntax.OpenBraceToken.IsMissing);
            Assert.Single(structDefinitionSyntax.OpenBraceToken.LeadingTrivia);
            Assert.False(structDefinitionSyntax.CloseBraceToken.IsMissing);

            Assert.False(typeDeclarationSyntax.SemicolonToken.IsMissing);
        }

        [Fact]
        public void HandlesMultipleExtraTokens()
        {
            var ast = BuildSyntaxTree("struct s t { { int a; }; int b;");

            Assert.NotNull(ast);
            Assert.Equal(3, ast.ChildNodes.Count);
            Assert.IsType<TypeDeclarationStatementSyntax>(ast.ChildNodes[0]);

            var typeDeclarationSyntax = (TypeDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.Equal(SyntaxKind.StructType, typeDeclarationSyntax.Type.Kind);

            var structDefinitionSyntax = (StructTypeSyntax)typeDeclarationSyntax.Type;
            Assert.False(structDefinitionSyntax.StructKeyword.IsMissing);
            Assert.False(structDefinitionSyntax.Name.IsMissing);
            Assert.False(structDefinitionSyntax.OpenBraceToken.IsMissing);
            Assert.Single(structDefinitionSyntax.OpenBraceToken.LeadingTrivia);
            Assert.False(structDefinitionSyntax.CloseBraceToken.IsMissing);

            Assert.False(typeDeclarationSyntax.SemicolonToken.IsMissing);

            Assert.IsType< VariableDeclarationStatementSyntax>(ast.ChildNodes[1]);
            AssertNodeKind(ast.ChildNodes[2], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesIncompleteDeclaration()
        {
            var ast = BuildSyntaxTree("struct s { int a; }; b");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.TypeDeclarationStatement);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
            var eof = (SyntaxToken) ast.ChildNodes[1];
            Assert.Single(eof.LeadingTrivia);
            Assert.Equal("b", eof.LeadingTrivia[0].ToString());
        }

        [Fact]
        public void HandlesGarbageAtEndOfFile()
        {
            var ast = BuildSyntaxTree("struct s { int a; }; b $");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.TypeDeclarationStatement);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
            var eof = (SyntaxToken)ast.ChildNodes[1];
            Assert.Equal(2, eof.LeadingTrivia.Length);
            Assert.Equal("b", eof.LeadingTrivia[0].ToString());
            Assert.Equal("$", eof.LeadingTrivia[1].ToString());
        }

        [Fact]
        public void HandlesSingleInvalidToken()
        {
            var ast = BuildSyntaxTree("0");

            Assert.NotNull(ast);
            Assert.Equal(1, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.EndOfFileToken);
            Assert.Single(((SyntaxToken)ast.ChildNodes[0]).LeadingTrivia);
        }

        [Fact]
        public void HandlesIncompleteStruct()
        {
            var ast = BuildSyntaxTree("struct s {");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.TypeDeclarationStatement);
            var typeDeclarationStatement = (TypeDeclarationStatementSyntax) ast.ChildNodes[0];
            Assert.Equal(SyntaxKind.StructType, typeDeclarationStatement.Type.Kind);
            var sd = (StructTypeSyntax) typeDeclarationStatement.Type;
            Assert.False(sd.OpenBraceToken.IsMissing);
            Assert.True(sd.CloseBraceToken.IsMissing);
            Assert.Empty(sd.Members);
            Assert.True(typeDeclarationStatement.SemicolonToken.IsMissing);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesIncompleteClass()
        {
            var ast = BuildSyntaxTree("class c {");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.TypeDeclarationStatement);
            var typeDeclarationStatement = (TypeDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.Equal(SyntaxKind.ClassType, typeDeclarationStatement.Type.Kind);
            var sd = (StructTypeSyntax)typeDeclarationStatement.Type;
            Assert.False(sd.OpenBraceToken.IsMissing);
            Assert.True(sd.CloseBraceToken.IsMissing);
            Assert.Empty(sd.Members);
            Assert.True(typeDeclarationStatement.SemicolonToken.IsMissing);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesIncompleteInterface()
        {
            var ast = BuildSyntaxTree("interface i {");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.TypeDeclarationStatement);
            var typeDeclarationStatement = (TypeDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.Equal(SyntaxKind.InterfaceType, typeDeclarationStatement.Type.Kind);
            var sd = (InterfaceTypeSyntax)typeDeclarationStatement.Type;
            Assert.False(sd.OpenBraceToken.IsMissing);
            Assert.True(sd.CloseBraceToken.IsMissing);
            Assert.Empty(sd.Methods);
            Assert.True(typeDeclarationStatement.SemicolonToken.IsMissing);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesIncompleteCBuffer()
        {
            var ast = BuildSyntaxTree("cbuffer cb {");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.ConstantBufferDeclaration);
            var sd = (ConstantBufferSyntax)ast.ChildNodes[0];
            Assert.False(sd.OpenBraceToken.IsMissing);
            Assert.True(sd.CloseBraceToken.IsMissing);
            Assert.Null(sd.SemicolonToken);
            Assert.Empty(sd.Declarations);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesIncompleteSamplerState()
        {
            var ast = BuildSyntaxTree("SamplerState s {");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.VariableDeclarationStatement);

            var sd = (VariableDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.False(sd.Declaration.IsMissing);
            Assert.Equal("SamplerState", sd.Declaration.Type.ToString());
            Assert.Single(sd.Declaration.Variables);
            Assert.Equal("s", sd.Declaration.Variables[0].Identifier.ToString());
            Assert.Equal(SyntaxKind.StateInitializer, sd.Declaration.Variables[0].Initializer.Kind);

            var initializer = (StateInitializerSyntax) sd.Declaration.Variables[0].Initializer;
            Assert.False(initializer.OpenBraceToken.IsMissing);
            Assert.Empty(initializer.Properties);
            Assert.True(initializer.CloseBraceToken.IsMissing);
            Assert.True(sd.SemicolonToken.IsMissing);

            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesIncompleteFunctionDeclaration()
        {
            var ast = BuildSyntaxTree("void main(");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.FunctionDeclaration);
            var sd = (FunctionDeclarationSyntax)ast.ChildNodes[0];
            Assert.Equal("void", sd.ReturnType.ToString());
            Assert.Equal("main", sd.Name.ToString());
            Assert.False(sd.ParameterList.OpenParenToken.IsMissing);
            Assert.Empty(sd.ParameterList.Parameters);
            Assert.True(sd.ParameterList.CloseParenToken.IsMissing);
            Assert.True(sd.SemicolonToken.IsMissing);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesIncompleteFunctionDeclarationWithSubsequentFunctionDeclaration()
        {
            var ast = BuildSyntaxTree(@"void main(
                float4 PS(float a) {}");

            Assert.NotNull(ast);
            Assert.Equal(3, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.FunctionDeclaration);
            var sd = (FunctionDeclarationSyntax)ast.ChildNodes[0];
            Assert.Equal("void", sd.ReturnType.ToString());
            Assert.Equal("main", sd.Name.ToString());
            Assert.False(sd.ParameterList.OpenParenToken.IsMissing);
            Assert.Empty(sd.ParameterList.Parameters);
            Assert.True(sd.ParameterList.CloseParenToken.IsMissing);
            Assert.True(sd.SemicolonToken.IsMissing);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.FunctionDefinition);
            AssertNodeKind(ast.ChildNodes[2], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesIncompleteFunctionDeclarationWithSubsequentFunctionDeclarationWithNoArguments()
        {
            var ast = BuildSyntaxTree(@"void main(
                float4 PS() {}");

            Assert.NotNull(ast);
            Assert.Equal(3, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.FunctionDeclaration);
            var sd = (FunctionDeclarationSyntax)ast.ChildNodes[0];
            Assert.Equal("void", sd.ReturnType.ToString());
            Assert.Equal("main", sd.Name.ToString());
            Assert.False(sd.ParameterList.OpenParenToken.IsMissing);
            Assert.Empty(sd.ParameterList.Parameters);
            Assert.True(sd.ParameterList.CloseParenToken.IsMissing);
            Assert.True(sd.SemicolonToken.IsMissing);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.FunctionDefinition);
            AssertNodeKind(ast.ChildNodes[2], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesFunctionDeclarationWithInvalidStructParameter()
        {
            var ast = BuildSyntaxTree("void main(int a, StructA StructB b, int c);");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.FunctionDeclaration);
            var sd = (FunctionDeclarationSyntax) ast.ChildNodes[0];
            Assert.Equal("void", sd.ReturnType.ToString());
            Assert.Equal("main", sd.Name.ToString());
            Assert.False(sd.ParameterList.OpenParenToken.IsMissing);
            Assert.NotEmpty(sd.ParameterList.Parameters);
            Assert.Equal(3, sd.ParameterList.Parameters.Count);
            Assert.False(sd.ParameterList.Parameters[0].ContainsDiagnostics);
            Assert.False(sd.ParameterList.Parameters.GetSeparator(0).ContainsDiagnostics);
            Assert.False(sd.ParameterList.Parameters[1].ContainsDiagnostics);
            Assert.True(sd.ParameterList.Parameters.GetSeparator(1).ContainsDiagnostics);
            Assert.Single(((SyntaxToken) sd.ParameterList.Parameters.GetSeparator(1)).LeadingTrivia);
            Assert.Equal(SyntaxKind.SkippedTokensTrivia, ((SyntaxToken)sd.ParameterList.Parameters.GetSeparator(1)).LeadingTrivia[0].Kind);
            Assert.Single(((SkippedTokensTriviaSyntax)((SyntaxToken)sd.ParameterList.Parameters.GetSeparator(1)).LeadingTrivia[0]).Tokens);
            Assert.Equal(SyntaxKind.IdentifierToken, ((SkippedTokensTriviaSyntax)((SyntaxToken)sd.ParameterList.Parameters.GetSeparator(1)).LeadingTrivia[0]).Tokens[0].Kind);
            Assert.Equal("b", ((SkippedTokensTriviaSyntax) ((SyntaxToken)sd.ParameterList.Parameters.GetSeparator(1)).LeadingTrivia[0]).Tokens[0].Text);
            Assert.False(sd.ParameterList.Parameters[2].ContainsDiagnostics);
            Assert.False(sd.ParameterList.CloseParenToken.IsMissing);
            Assert.False(sd.SemicolonToken.IsMissing);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesFunctionDeclarationWithInvalidIntrinsicParameter()
        {
            var ast = BuildSyntaxTree("void main(int a, int int b, int c);");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.FunctionDeclaration);
            var sd = (FunctionDeclarationSyntax)ast.ChildNodes[0];
            Assert.Equal("void", sd.ReturnType.ToString());
            Assert.Equal("main", sd.Name.ToString());
            Assert.False(sd.ParameterList.OpenParenToken.IsMissing);
            Assert.NotEmpty(sd.ParameterList.Parameters);
            Assert.Equal(3, sd.ParameterList.Parameters.Count);
            Assert.False(sd.ParameterList.Parameters[0].ContainsDiagnostics);
            Assert.False(sd.ParameterList.Parameters.GetSeparator(0).ContainsDiagnostics);
            Assert.True(sd.ParameterList.Parameters[1].ContainsDiagnostics);
            Assert.False(sd.ParameterList.Parameters[1].Type.ContainsDiagnostics);
            Assert.Single(sd.ParameterList.Parameters[1].Declarator.Identifier.LeadingTrivia);
            Assert.Equal(SyntaxKind.SkippedTokensTrivia, sd.ParameterList.Parameters[1].Declarator.Identifier.LeadingTrivia[0].Kind);
            Assert.Equal(SyntaxKind.IntKeyword, ((SkippedTokensTriviaSyntax) sd.ParameterList.Parameters[1].Declarator.Identifier.LeadingTrivia[0]).Tokens[0].Kind);
            Assert.False(sd.ParameterList.Parameters.GetSeparator(1).ContainsDiagnostics);
            Assert.False(sd.ParameterList.Parameters[2].ContainsDiagnostics);
            Assert.False(sd.ParameterList.CloseParenToken.IsMissing);
            Assert.False(sd.SemicolonToken.IsMissing);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesInvalidArrayDeclaration()
        {
            var ast = BuildSyntaxTree(@"h []");

            Assert.NotNull(ast);
            Assert.Equal(1, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesIncompleteArrayInitializer()
        {
            var ast = BuildSyntaxTree("int a[] = { 0 ");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.VariableDeclarationStatement);
            var sd = (VariableDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.Single(sd.Declaration.Variables);
            Assert.Equal(SyntaxKind.EqualsValueClause, sd.Declaration.Variables[0].Initializer.Kind);
            var initializer = (EqualsValueClauseSyntax) sd.Declaration.Variables[0].Initializer;
            Assert.False(initializer.EqualsToken.IsMissing);
            Assert.Equal(SyntaxKind.ArrayInitializerExpression, initializer.Value.Kind);
            Assert.True(sd.SemicolonToken.IsMissing);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesIncompleteForStatementExpressionList()
        {
            var ast = BuildSyntaxTree("void main() { for (i = 0, }");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.FunctionDefinition);
            var sd = (FunctionDefinitionSyntax)ast.ChildNodes[0];
            Assert.Equal("void", sd.ReturnType.ToString());
            Assert.Equal("main", sd.Name.ToString());
            Assert.False(sd.ParameterList.OpenParenToken.IsMissing);
            Assert.Empty(sd.ParameterList.Parameters);
            Assert.Single(sd.Body.Statements);
            Assert.Equal(SyntaxKind.ForStatement, sd.Body.Statements[0].Kind);
            var forStatement = (ForStatementSyntax) sd.Body.Statements[0];
            Assert.NotNull(forStatement.Initializer);
            Assert.Equal(SyntaxKind.CompoundExpression, forStatement.Initializer.Kind);
            var compExpr = (CompoundExpressionSyntax) forStatement.Initializer;
            Assert.Equal(SyntaxKind.SimpleAssignmentExpression, compExpr.Left.Kind);
            Assert.True(compExpr.Right.IsMissing);
            Assert.Equal(SyntaxKind.IdentifierName, compExpr.Right.Kind);
            Assert.False(sd.ParameterList.CloseParenToken.IsMissing);
            Assert.Null(sd.SemicolonToken);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void HandlesExtraForStatementIncrementorList()
        {
            var ast = BuildSyntaxTree("void main() { for (i = 0; i < 10; i++_) {}}");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.FunctionDefinition);
            var sd = (FunctionDefinitionSyntax)ast.ChildNodes[0];
            Assert.Equal("void", sd.ReturnType.ToString());
            Assert.Equal("main", sd.Name.ToString());
            Assert.False(sd.ParameterList.OpenParenToken.IsMissing);
            Assert.Empty(sd.ParameterList.Parameters);
            Assert.Single(sd.Body.Statements);
            Assert.Equal(SyntaxKind.ForStatement, sd.Body.Statements[0].Kind);
            var forStatement = (ForStatementSyntax) sd.Body.Statements[0];
            Assert.NotNull(forStatement.Initializer);
            Assert.Equal(SyntaxKind.SimpleAssignmentExpression, forStatement.Initializer.Kind);
            Assert.False(sd.ParameterList.CloseParenToken.IsMissing);
            Assert.Null(sd.SemicolonToken);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void CorrectlyReportsInvalidIdentifier()
        {
            var ast = BuildSyntaxTree("float 4; float f3;");

            Assert.NotNull(ast);
            Assert.Equal(3, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.VariableDeclarationStatement);
            var vds = (VariableDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.Equal("float", vds.Declaration.Type.ToString());
            Assert.True(vds.Declaration.Variables[0].Identifier.IsMissing);
            Assert.Single(vds.Declaration.Variables[0].Identifier.Diagnostics);
            Assert.Single(vds.SemicolonToken.LeadingTrivia);
            Assert.Equal(SyntaxKind.SkippedTokensTrivia, vds.SemicolonToken.LeadingTrivia[0].Kind);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.VariableDeclarationStatement);
            AssertNodeKind(ast.ChildNodes[2], SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void CorrectlyReportsInvalidKeyword()
        {
            var ast = BuildSyntaxTree("int f; in");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.VariableDeclarationStatement);
            var vds = (VariableDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.Equal("int", vds.Declaration.Type.ToString());
            Assert.False(vds.Declaration.Variables[0].Identifier.IsMissing);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
            Assert.Single(ast.ChildNodes[1].Diagnostics);
            Assert.Single(((SyntaxToken) ast.ChildNodes[1]).LeadingTrivia);
            Assert.Equal(SyntaxKind.SkippedTokensTrivia, ((SyntaxToken) ast.ChildNodes[1]).LeadingTrivia[0].Kind);
        }

        [Fact]
        public void CorrectlyReportsSkippedTokens()
        {
            var ast = BuildSyntaxTree("4 4 4; float f3;");

            Assert.NotNull(ast);
            Assert.Equal(2, ast.ChildNodes.Count);
            AssertNodeKind(ast.ChildNodes[0], SyntaxKind.VariableDeclarationStatement);
            var vds = (VariableDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.Single(vds.Declaration.Type.GetFirstTokenInDescendants().LeadingTrivia);
            Assert.Equal(SyntaxKind.SkippedTokensTrivia, vds.Declaration.Type.GetFirstTokenInDescendants().LeadingTrivia[0].Kind);
            Assert.Equal(4, ((SkippedTokensTriviaSyntax) vds.Declaration.Type.GetFirstTokenInDescendants().LeadingTrivia[0]).Tokens.Count);
            Assert.Single(vds.Declaration.Type.GetFirstTokenInDescendants().Diagnostics);
            Assert.Equal("Unexpected token '4'.", vds.Declaration.Type.GetFirstTokenInDescendants().Diagnostics[0].Message);
            AssertNodeKind(ast.ChildNodes[1], SyntaxKind.EndOfFileToken);
        }

        private static void AssertNodeKind(SyntaxNodeBase node, SyntaxKind kind)
        {
            Assert.Equal((ushort) kind, node.RawKind);
        }

        [Theory]
        [MemberData(nameof(GetInProgressMethodCode))]
        public void HandlesTypingMethod(string code)
        {
            var ast = BuildSyntaxTree(code);

            Assert.NotNull(ast);
        }

        public static IEnumerable<object[]> GetInProgressMethodCode()
        {
            const string code = @"void main()
{
    [unroll]
    for (int i = 0; i < 3; i++)
    {
        foo();
        bar(3);

        float f = 3 + 4 * 5;
        float 2[] = { 1, 2 };

        [branch]
        break;
    }
}";

            for (var i = 1; i <= code.Length; i++)
                yield return new object[] { code.Substring(0, i) };
        }

        private static CompilationUnitSyntax BuildSyntaxTree(string code)
        {
            var compilationUnit = SyntaxFactory.ParseCompilationUnit(new SourceFile(SourceText.From(code)));
            Assert.Equal(code, compilationUnit.ToFullString());
            return compilationUnit;
        }
    }
}