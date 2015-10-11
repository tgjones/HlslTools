using System.Collections.Generic;
using HlslTools.Syntax;
using NUnit.Framework;

namespace HlslTools.Tests.Parser
{
    [TestFixture]
    public class ErrorRecoveryTests
    {
        [Test]
        public void HandlesSingleMissingToken()
        {
            var ast = BuildSyntaxTree("struct s int a; };");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0], Is.TypeOf<TypeDeclarationStatementSyntax>());

            var typeDeclarationSyntax = (TypeDeclarationStatementSyntax) ast.ChildNodes[0];
            Assert.That(typeDeclarationSyntax.Type.Kind, Is.EqualTo(SyntaxKind.StructType));

            var structDefinitionSyntax = (StructTypeSyntax)typeDeclarationSyntax.Type;
            Assert.That(structDefinitionSyntax.StructKeyword.IsMissing, Is.False);
            Assert.That(structDefinitionSyntax.Name.IsMissing, Is.False);
            Assert.That(structDefinitionSyntax.OpenBraceToken.IsMissing, Is.True);
            Assert.That(structDefinitionSyntax.CloseBraceToken.IsMissing, Is.False);

            Assert.That(typeDeclarationSyntax.SemicolonToken.IsMissing, Is.False);
        }

        [Test]
        public void HandlesSingleExtraToken()
        {
            var ast = BuildSyntaxTree("struct s t { int a; };");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0], Is.TypeOf<TypeDeclarationStatementSyntax>());

            var typeDeclarationSyntax = (TypeDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.That(typeDeclarationSyntax.Type.Kind, Is.EqualTo(SyntaxKind.StructType));

            var structDefinitionSyntax = (StructTypeSyntax)typeDeclarationSyntax.Type;
            Assert.That(structDefinitionSyntax.StructKeyword.IsMissing, Is.False);
            Assert.That(structDefinitionSyntax.Name.IsMissing, Is.False);
            Assert.That(structDefinitionSyntax.OpenBraceToken.IsMissing, Is.False);
            Assert.That(structDefinitionSyntax.OpenBraceToken.LeadingTrivia, Has.Length.EqualTo(1));
            Assert.That(structDefinitionSyntax.CloseBraceToken.IsMissing, Is.False);

            Assert.That(typeDeclarationSyntax.SemicolonToken.IsMissing, Is.False);
        }

        [Test]
        public void HandlesMultipleExtraTokens()
        {
            var ast = BuildSyntaxTree("struct s t { { int a; }; int b;");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(3));
            Assert.That(ast.ChildNodes[0], Is.TypeOf<TypeDeclarationStatementSyntax>());

            var typeDeclarationSyntax = (TypeDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.That(typeDeclarationSyntax.Type.Kind, Is.EqualTo(SyntaxKind.StructType));

            var structDefinitionSyntax = (StructTypeSyntax)typeDeclarationSyntax.Type;
            Assert.That(structDefinitionSyntax.StructKeyword.IsMissing, Is.False);
            Assert.That(structDefinitionSyntax.Name.IsMissing, Is.False);
            Assert.That(structDefinitionSyntax.OpenBraceToken.IsMissing, Is.False);
            Assert.That(structDefinitionSyntax.OpenBraceToken.LeadingTrivia, Has.Length.EqualTo(1));
            Assert.That(structDefinitionSyntax.CloseBraceToken.IsMissing, Is.False);

            Assert.That(typeDeclarationSyntax.SemicolonToken.IsMissing, Is.False);

            Assert.That(ast.ChildNodes[1], Is.TypeOf<VariableDeclarationStatementSyntax>());
            Assert.That(ast.ChildNodes[2].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesIncompleteDeclaration()
        {
            var ast = BuildSyntaxTree("struct s { int a; }; b");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.TypeDeclarationStatement));
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
            var eof = (SyntaxToken) ast.ChildNodes[1];
            Assert.That(eof.LeadingTrivia, Has.Length.EqualTo(1));
            Assert.That(eof.LeadingTrivia[0].ToString(), Is.EqualTo("b"));
        }

        [Test]
        public void HandlesGarbageAtEndOfFile()
        {
            var ast = BuildSyntaxTree("struct s { int a; }; b $");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.TypeDeclarationStatement));
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
            var eof = (SyntaxToken)ast.ChildNodes[1];
            Assert.That(eof.LeadingTrivia, Has.Length.EqualTo(2));
            Assert.That(eof.LeadingTrivia[0].ToString(), Is.EqualTo("b"));
            Assert.That(eof.LeadingTrivia[1].ToString(), Is.EqualTo("$"));
        }

        [Test]
        public void HandlesSingleInvalidToken()
        {
            var ast = BuildSyntaxTree("0");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(1));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
            Assert.That(((SyntaxToken)ast.ChildNodes[0]).LeadingTrivia, Has.Length.EqualTo(1));
        }

        [Test]
        public void HandlesIncompleteStruct()
        {
            var ast = BuildSyntaxTree("struct s {");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.TypeDeclarationStatement));
            var typeDeclarationStatement = (TypeDeclarationStatementSyntax) ast.ChildNodes[0];
            Assert.That(typeDeclarationStatement.Type.Kind, Is.EqualTo(SyntaxKind.StructType));
            var sd = (StructTypeSyntax) typeDeclarationStatement.Type;
            Assert.That(sd.OpenBraceToken.IsMissing, Is.False);
            Assert.That(sd.CloseBraceToken.IsMissing, Is.True);
            Assert.That(sd.Fields, Has.Count.EqualTo(0));
            Assert.That(typeDeclarationStatement.SemicolonToken.IsMissing, Is.True);
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesIncompleteClass()
        {
            var ast = BuildSyntaxTree("class c {");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.TypeDeclarationStatement));
            var typeDeclarationStatement = (TypeDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.That(typeDeclarationStatement.Type.Kind, Is.EqualTo(SyntaxKind.ClassType));
            var sd = (ClassTypeSyntax)typeDeclarationStatement.Type;
            Assert.That(sd.OpenBraceToken.IsMissing, Is.False);
            Assert.That(sd.CloseBraceToken.IsMissing, Is.True);
            Assert.That(sd.Members, Has.Count.EqualTo(0));
            Assert.That(typeDeclarationStatement.SemicolonToken.IsMissing, Is.True);
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesIncompleteInterface()
        {
            var ast = BuildSyntaxTree("interface i {");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.TypeDeclarationStatement));
            var typeDeclarationStatement = (TypeDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.That(typeDeclarationStatement.Type.Kind, Is.EqualTo(SyntaxKind.InterfaceType));
            var sd = (InterfaceTypeSyntax)typeDeclarationStatement.Type;
            Assert.That(sd.OpenBraceToken.IsMissing, Is.False);
            Assert.That(sd.CloseBraceToken.IsMissing, Is.True);
            Assert.That(sd.Methods, Has.Count.EqualTo(0));
            Assert.That(typeDeclarationStatement.SemicolonToken.IsMissing, Is.True);
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesIncompleteCBuffer()
        {
            var ast = BuildSyntaxTree("cbuffer cb {");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.ConstantBufferDeclaration));
            var sd = (ConstantBufferSyntax)ast.ChildNodes[0];
            Assert.That(sd.OpenBraceToken.IsMissing, Is.False);
            Assert.That(sd.CloseBraceToken.IsMissing, Is.True);
            Assert.That(sd.SemicolonToken, Is.Null);
            Assert.That(sd.Declarations, Has.Count.EqualTo(0));
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesIncompleteSamplerState()
        {
            var ast = BuildSyntaxTree("SamplerState s {");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));
            var sd = (VariableDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.That(sd.Declaration.IsMissing, Is.False);
            Assert.That(sd.Declaration.Type.ToString(), Is.EqualTo("SamplerState"));
            Assert.That(sd.Declaration.Variables, Has.Count.EqualTo(1));
            Assert.That(sd.Declaration.Variables[0].Identifier.ToString(), Is.EqualTo("s"));
            Assert.That(sd.Declaration.Variables[0].Initializer.Kind, Is.EqualTo(SyntaxKind.StateInitializer));
            var initializer = (StateInitializerSyntax) sd.Declaration.Variables[0].Initializer;
            Assert.That(initializer.OpenBraceToken.IsMissing, Is.False);
            Assert.That(initializer.Properties.Count, Is.EqualTo(0));
            Assert.That(initializer.CloseBraceToken.IsMissing, Is.True);
            Assert.That(sd.SemicolonToken.IsMissing, Is.True);
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesIncompleteFunctionDeclaration()
        {
            var ast = BuildSyntaxTree("void main(");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.FunctionDeclaration));
            var sd = (FunctionDeclarationSyntax)ast.ChildNodes[0];
            Assert.That(sd.ReturnType.ToString(), Is.EqualTo("void"));
            Assert.That(sd.Name.ToString(), Is.EqualTo("main"));
            Assert.That(sd.ParameterList.OpenParenToken.IsMissing, Is.False);
            Assert.That(sd.ParameterList.Parameters, Has.Count.EqualTo(0));
            Assert.That(sd.ParameterList.CloseParenToken.IsMissing, Is.True);
            Assert.That(sd.SemicolonToken.IsMissing, Is.True);
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesIncompleteFunctionDeclarationWithSubsequentFunctionDeclaration()
        {
            var ast = BuildSyntaxTree(@"void main(
                float4 PS(float a) {}");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(3));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.FunctionDeclaration));
            var sd = (FunctionDeclarationSyntax)ast.ChildNodes[0];
            Assert.That(sd.ReturnType.ToString(), Is.EqualTo("void"));
            Assert.That(sd.Name.ToString(), Is.EqualTo("main"));
            Assert.That(sd.ParameterList.OpenParenToken.IsMissing, Is.False);
            Assert.That(sd.ParameterList.Parameters, Has.Count.EqualTo(0));
            Assert.That(sd.ParameterList.CloseParenToken.IsMissing, Is.True);
            Assert.That(sd.SemicolonToken.IsMissing, Is.True);
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.FunctionDefinition));
            Assert.That(ast.ChildNodes[2].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesIncompleteFunctionDeclarationWithSubsequentFunctionDeclarationWithNoArguments()
        {
            var ast = BuildSyntaxTree(@"void main(
                float4 PS() {}");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(3));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.FunctionDeclaration));
            var sd = (FunctionDeclarationSyntax)ast.ChildNodes[0];
            Assert.That(sd.ReturnType.ToString(), Is.EqualTo("void"));
            Assert.That(sd.Name.ToString(), Is.EqualTo("main"));
            Assert.That(sd.ParameterList.OpenParenToken.IsMissing, Is.False);
            Assert.That(sd.ParameterList.Parameters, Has.Count.EqualTo(0));
            Assert.That(sd.ParameterList.CloseParenToken.IsMissing, Is.True);
            Assert.That(sd.SemicolonToken.IsMissing, Is.True);
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.FunctionDefinition));
            Assert.That(ast.ChildNodes[2].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesInvalidArrayDeclaration()
        {
            var ast = BuildSyntaxTree(@"h []");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(1));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesIncompleteArrayInitializer()
        {
            var ast = BuildSyntaxTree("int a[] = { 0 ");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));
            var sd = (VariableDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.That(sd.Declaration.Variables, Has.Count.EqualTo(1));
            Assert.That(sd.Declaration.Variables[0].Initializer.Kind, Is.EqualTo(SyntaxKind.EqualsValueClause));
            var initializer = (EqualsValueClauseSyntax) sd.Declaration.Variables[0].Initializer;
            Assert.That(initializer.EqualsToken.IsMissing, Is.False);
            Assert.That(initializer.Value.Kind, Is.EqualTo(SyntaxKind.ArrayInitializerExpression));
            Assert.That(sd.SemicolonToken.IsMissing, Is.True);
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesIncompleteForStatementExpressionList()
        {
            var ast = BuildSyntaxTree("void main() { for (i = 0, }");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.FunctionDefinition));
            var sd = (FunctionDefinitionSyntax)ast.ChildNodes[0];
            Assert.That(sd.ReturnType.ToString(), Is.EqualTo("void"));
            Assert.That(sd.Name.ToString(), Is.EqualTo("main"));
            Assert.That(sd.ParameterList.OpenParenToken.IsMissing, Is.False);
            Assert.That(sd.ParameterList.Parameters, Has.Count.EqualTo(0));
            Assert.That(sd.Body.Statements, Has.Count.EqualTo(1));
            Assert.That(sd.Body.Statements[0].Kind, Is.EqualTo(SyntaxKind.ForStatement));
            var forStatement = (ForStatementSyntax) sd.Body.Statements[0];
            Assert.That(forStatement.Initializer, Is.Not.Null);
            Assert.That(forStatement.Initializer.Kind, Is.EqualTo(SyntaxKind.CompoundExpression));
            var compExpr = (CompoundExpressionSyntax) forStatement.Initializer;
            Assert.That(compExpr.Left.Kind, Is.EqualTo(SyntaxKind.SimpleAssignmentExpression));
            Assert.That(compExpr.Right.IsMissing, Is.True);
            Assert.That(compExpr.Right.Kind, Is.EqualTo(SyntaxKind.IdentifierName));
            Assert.That(sd.ParameterList.CloseParenToken.IsMissing, Is.False);
            Assert.That(sd.SemicolonToken, Is.Null);
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void HandlesExtraForStatementIncrementorList()
        {
            var ast = BuildSyntaxTree("void main() { for (i = 0; i < 10; i++_) {}}");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.FunctionDefinition));
            var sd = (FunctionDefinitionSyntax)ast.ChildNodes[0];
            Assert.That(sd.ReturnType.ToString(), Is.EqualTo("void"));
            Assert.That(sd.Name.ToString(), Is.EqualTo("main"));
            Assert.That(sd.ParameterList.OpenParenToken.IsMissing, Is.False);
            Assert.That(sd.ParameterList.Parameters, Has.Count.EqualTo(0));
            Assert.That(sd.Body.Statements, Has.Count.EqualTo(1));
            Assert.That(sd.Body.Statements[0].Kind, Is.EqualTo(SyntaxKind.ForStatement));
            var forStatement = (ForStatementSyntax) sd.Body.Statements[0];
            Assert.That(forStatement.Initializer, Is.Not.Null);
            Assert.That(forStatement.Initializer.Kind, Is.EqualTo(SyntaxKind.SimpleAssignmentExpression));
            Assert.That(sd.ParameterList.CloseParenToken.IsMissing, Is.False);
            Assert.That(sd.SemicolonToken, Is.Null);
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void CorrectlyReportsInvalidIdentifier()
        {
            var ast = BuildSyntaxTree("float 4; float f3;");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(3));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));
            var vds = (VariableDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.That(vds.Declaration.Type.ToString(), Is.EqualTo("float"));
            Assert.That(vds.Declaration.Variables[0].Identifier.IsMissing, Is.True);
            Assert.That(vds.Declaration.Variables[0].Identifier.Diagnostics, Has.Length.EqualTo(1));
            Assert.That(vds.SemicolonToken.LeadingTrivia, Has.Length.EqualTo(1));
            Assert.That(vds.SemicolonToken.LeadingTrivia[0].Kind, Is.EqualTo(SyntaxKind.SkippedTokensTrivia));
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));
            Assert.That(ast.ChildNodes[2].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [Test]
        public void CorrectlyReportsInvalidKeyword()
        {
            var ast = BuildSyntaxTree("int f; in");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));
            var vds = (VariableDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.That(vds.Declaration.Type.ToString(), Is.EqualTo("int"));
            Assert.That(vds.Declaration.Variables[0].Identifier.IsMissing, Is.False);
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
            Assert.That(ast.ChildNodes[1].Diagnostics, Has.Length.EqualTo(1));
            Assert.That(((SyntaxToken) ast.ChildNodes[1]).LeadingTrivia, Has.Length.EqualTo(1));
            Assert.That(((SyntaxToken) ast.ChildNodes[1]).LeadingTrivia[0].Kind, Is.EqualTo(SyntaxKind.SkippedTokensTrivia));
        }

        [Test]
        public void CorrectlyReportsSkippedTokens()
        {
            var ast = BuildSyntaxTree("4 4 4; float f3;");

            Assert.That(ast, Is.Not.Null);
            Assert.That(ast.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(ast.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));
            var vds = (VariableDeclarationStatementSyntax)ast.ChildNodes[0];
            Assert.That(vds.Declaration.Type.GetFirstTokenInDescendants().LeadingTrivia, Has.Length.EqualTo(1));
            Assert.That(vds.Declaration.Type.GetFirstTokenInDescendants().LeadingTrivia[0].Kind, Is.EqualTo(SyntaxKind.SkippedTokensTrivia));
            Assert.That(((SkippedTokensTriviaSyntax) vds.Declaration.Type.GetFirstTokenInDescendants().LeadingTrivia[0]).Tokens, Has.Count.EqualTo(4));
            Assert.That(vds.Declaration.Type.GetFirstTokenInDescendants().Diagnostics, Has.Length.EqualTo(1));
            Assert.That(vds.Declaration.Type.GetFirstTokenInDescendants().Diagnostics[0].Message, Is.EqualTo("Unexpected token '4'."));
            Assert.That(ast.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
        }

        [TestCaseSource(nameof(GetInProgressMethodCode))]
        public void HandlesTypingMethod(string code)
        {
            var ast = BuildSyntaxTree(code);

            Assert.That(ast, Is.Not.Null);
        }

        private static IEnumerable<TestCaseData> GetInProgressMethodCode()
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
                yield return new TestCaseData(code.Substring(0, i));
        }

        private static CompilationUnitSyntax BuildSyntaxTree(string code)
        {
            var compilationUnit = SyntaxFactory.ParseCompilationUnit(code);
            Assert.That(compilationUnit.ToFullString(), Is.EqualTo(code));
            return compilationUnit;
        }
    }
}