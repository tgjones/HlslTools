using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Tests.Support;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Parser
{
    public class PreprocessorTests
    {
        [Fact]
        public void TestDefineAndTrueEqualityExpression()
        {
            const string text = @"
#define FOO 1
#if FOO == 1
int a;
#else
float b;
#endif
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive },
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.BranchTaken | NodeStatus.TrueValue },
                new DirectiveInfo { Kind = SyntaxKind.ElseDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestNegDefineWithNoValue()
        {
            const string text = @"
#define FOO
#if FOO
int a;
#endif
FOO
";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.InvalidExprTerm);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive },
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestNegDefineWithNoValueAndEqualityExpression()
        {
            const string text = @"
#define FOO
#if FOO == 0
int a;
#endif
";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.InvalidExprTerm);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive },
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestDefineAndArithmeticExpression()
        {
            const string text = @"
#define FOO 1
#if FOO == (2 + 1) * 3 - (FOO + 7)
int a;
#else
float b;
#endif
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive },
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.BranchTaken | NodeStatus.TrueValue },
                new DirectiveInfo { Kind = SyntaxKind.ElseDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestDefineAndFalseEqualityExpression()
        {
            const string text = @"
#define FOO 1
#if FOO == 2
int a;
#else
float b;
#endif
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive },
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue },
                new DirectiveInfo { Kind = SyntaxKind.ElseDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.BranchTaken },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestDefineWithParentheses()
        {
            const string text = @"
#define FOO (1.0/8.0)
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestDefineWithContinuation()
        {
            const string text = @"
#define FOO (1.0/ \
  8.0)
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestDefineWithKeywords()
        {
            const string text = "#define SAMPLE_TEXTURE(sampler, uv) (tex2D(sampler, uv))";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestMacroExpansionOrder()
        {
            const string text = @"
#define TABLESIZE BUFSIZE
#define BUFSIZE 1024
int i = TABLESIZE;
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive },
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive });

            Assert.Equal(2, node.ChildNodes.Count);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[0]).Kind);

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.Equal(SyntaxKind.PredefinedScalarType, varDeclStatement.Declaration.Type.Kind);
            Assert.Equal(1, varDeclStatement.Declaration.Variables.Count);
            Assert.Equal("i", varDeclStatement.Declaration.Variables[0].Identifier.Text);
            Assert.NotNull(varDeclStatement.Declaration.Variables[0].Initializer);
            Assert.Equal(SyntaxKind.EqualsValueClause, varDeclStatement.Declaration.Variables[0].Initializer.Kind);

            var equalsValueClause = (EqualsValueClauseSyntax)varDeclStatement.Declaration.Variables[0].Initializer;
            Assert.Equal(SyntaxKind.NumericLiteralExpression, equalsValueClause.Value.Kind);

            var numericExpr = (LiteralExpressionSyntax) equalsValueClause.Value;
            Assert.Equal("1024", numericExpr.Token.Text);
        }

        [Fact]
        public void TestFunctionLikeDefine()
        {
            const string text = @"
#define FOO(a, b) a + b
float f = FOO(1, 2);
float g = FOO(3, 4);
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive });

            Assert.Equal(3, node.ChildNodes.Count);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[0]).Kind);

            var varDeclStatement = (VariableDeclarationStatementSyntax) node.ChildNodes[0];
            Assert.Equal(SyntaxKind.PredefinedScalarType, varDeclStatement.Declaration.Type.Kind);
            Assert.Equal(1, varDeclStatement.Declaration.Variables.Count);
            Assert.Equal("f", varDeclStatement.Declaration.Variables[0].Identifier.Text);
            Assert.NotNull(varDeclStatement.Declaration.Variables[0].Initializer);
            Assert.Equal(SyntaxKind.EqualsValueClause, varDeclStatement.Declaration.Variables[0].Initializer.Kind);

            var equalsValueClause = (EqualsValueClauseSyntax) varDeclStatement.Declaration.Variables[0].Initializer;
            Assert.Equal(SyntaxKind.AddExpression, equalsValueClause.Value.Kind);

            var addExpr = (BinaryExpressionSyntax) equalsValueClause.Value;
            Assert.Equal(SyntaxKind.NumericLiteralExpression, addExpr.Left.Kind);
            Assert.Equal("1", ((LiteralExpressionSyntax) addExpr.Left).Token.Text);
            Assert.Equal(SyntaxKind.PlusToken, addExpr.OperatorToken.Kind);
            Assert.Equal(SyntaxKind.NumericLiteralExpression, addExpr.Right.Kind);
            Assert.Equal("2", ((LiteralExpressionSyntax)addExpr.Right).Token.Text);

            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[1]).Kind);
        }

        [Fact]
        public void TestNestedFunctionLikeDefines()
        {
            const string text = @"
    #define MULTIPLY(a, b) a * b
    #define MAD(a,b,c) MULTIPLY(a,b)+c
    int i = MAD(2, 3, 4);
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive },
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive });

            Assert.Equal(2, node.ChildNodes.Count);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[0]).Kind);

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.Equal(SyntaxKind.PredefinedScalarType, varDeclStatement.Declaration.Type.Kind);
            Assert.Equal(1, varDeclStatement.Declaration.Variables.Count);
            Assert.Equal("i", varDeclStatement.Declaration.Variables[0].Identifier.Text);
            Assert.NotNull(varDeclStatement.Declaration.Variables[0].Initializer);
            Assert.Equal(SyntaxKind.EqualsValueClause, varDeclStatement.Declaration.Variables[0].Initializer.Kind);

            var equalsValueClause = (EqualsValueClauseSyntax)varDeclStatement.Declaration.Variables[0].Initializer;
            Assert.Equal(SyntaxKind.AddExpression, equalsValueClause.Value.Kind);

            var addExpr = (BinaryExpressionSyntax)equalsValueClause.Value;
            Assert.Equal(SyntaxKind.MultiplyExpression, addExpr.Left.Kind);

            var mulExpr = (BinaryExpressionSyntax) addExpr.Left;
            Assert.Equal(SyntaxKind.NumericLiteralExpression, mulExpr.Left.Kind);
            Assert.Equal(SyntaxKind.AsteriskToken, mulExpr.OperatorToken.Kind);
            Assert.Equal(SyntaxKind.NumericLiteralExpression, mulExpr.Right.Kind);

            Assert.Equal(SyntaxKind.PlusToken, addExpr.OperatorToken.Kind);
            Assert.Equal(SyntaxKind.NumericLiteralExpression, addExpr.Right.Kind);
        }

        [Fact]
        public void TestNegNotEnoughParamsOnFunctionLikeMacroReference()
        {
            const string text = @"
    #define MULTIPLY(a, b) a * b
    int i = MULTIPLY(2);
";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.NotEnoughMacroParameters);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive });

            Assert.Equal(2, node.ChildNodes.Count);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[0]).Kind);

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.Equal(SyntaxKind.PredefinedScalarType, varDeclStatement.Declaration.Type.Kind);
            Assert.Equal(1, varDeclStatement.Declaration.Variables.Count);
            Assert.Equal("i", varDeclStatement.Declaration.Variables[0].Identifier.Text);
            Assert.NotNull(varDeclStatement.Declaration.Variables[0].Initializer);
            Assert.Equal(SyntaxKind.EqualsValueClause, varDeclStatement.Declaration.Variables[0].Initializer.Kind);

            var equalsValueClause = (EqualsValueClauseSyntax)varDeclStatement.Declaration.Variables[0].Initializer;
            Assert.Equal(SyntaxKind.IdentifierName, equalsValueClause.Value.Kind);

            var identName = (IdentifierNameSyntax)equalsValueClause.Value;
            Assert.Equal("MULTIPLY", identName.Name.Text);
        }

        [Fact]
        public void TestNegFunctionLikeDefineWithTokenPasteOperator()
        {
            const string text = @"
#define FOO
#define PARAM_DEFN(type, name) type name
PARAM_DEFN(float, Transparency##FOO) = 0.5;
";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.TokenUnexpected);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "FOO" },
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "PARAM_DEFN" },
                new DirectiveInfo { Kind = SyntaxKind.BadDirectiveTrivia, Status = NodeStatus.IsActive, Text = "##FOO) = 0.5;" });
        }

        [Fact]
        public void TestFunctionLikeDefineWithNestedPaste()
        {
            const string text = @"
#define FOO
#define PASTE(x, y) x##y
#define PARAM(type, name) type name
PARAM(float, PASTE(bar, baz)) = 1.0f;
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "FOO" },
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "PASTE" },
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "PARAM" });

            Assert.Equal(2, node.ChildNodes.Count);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[0]).Kind);

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.Equal(SyntaxKind.PredefinedScalarType, varDeclStatement.Declaration.Type.Kind);
            Assert.Equal(1, varDeclStatement.Declaration.Variables.Count);
            Assert.Equal("barbaz", varDeclStatement.Declaration.Variables[0].Identifier.Text);
            Assert.NotNull(varDeclStatement.Declaration.Variables[0].Initializer);
            Assert.Equal(SyntaxKind.EqualsValueClause, varDeclStatement.Declaration.Variables[0].Initializer.Kind);
        }

        [Fact]
        public void TestFunctionLikeDefineWithPasteWithMultiTokenArguments()
        {
            const string text = @"
#define FOO(name, uvcoords) Texture2D g_##name##Texture; \
    SamplerState name##Sampler; \
    float4 GetTex(PsInput input) { \
        return g_##name##Texture.Sample(name##Sampler, input.##uvcoords); \
    }

FOO(Diffuse, texCoords.xy)
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "FOO" });

            Assert.Equal(4, node.ChildNodes.Count);

            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[0]).Kind);
            var varDeclStatement1 = (VariableDeclarationStatementSyntax) node.ChildNodes[0];
            Assert.Equal(SyntaxKind.PredefinedObjectType, varDeclStatement1.Declaration.Type.Kind);
            Assert.Equal("Texture2D", ((PredefinedObjectTypeSyntax) varDeclStatement1.Declaration.Type).ObjectTypeToken.Text);
            Assert.Equal(1, varDeclStatement1.Declaration.Variables.Count);
            Assert.Equal("g_DiffuseTexture", varDeclStatement1.Declaration.Variables[0].Identifier.Text);
            Assert.Null(varDeclStatement1.Declaration.Variables[0].Initializer);

            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[1]).Kind);
            var varDeclStatement2 = (VariableDeclarationStatementSyntax)node.ChildNodes[1];
            Assert.Equal(SyntaxKind.PredefinedObjectType, varDeclStatement2.Declaration.Type.Kind);
            Assert.Equal("SamplerState", ((PredefinedObjectTypeSyntax)varDeclStatement2.Declaration.Type).ObjectTypeToken.Text);
            Assert.Equal(1, varDeclStatement2.Declaration.Variables.Count);
            Assert.Equal("DiffuseSampler", varDeclStatement2.Declaration.Variables[0].Identifier.Text);
            Assert.Null(varDeclStatement2.Declaration.Variables[0].Initializer);

            Assert.Equal(SyntaxKind.FunctionDefinition, ((SyntaxNode) node.ChildNodes[2]).Kind);
            var funcDefStatement = (FunctionDefinitionSyntax) node.ChildNodes[2];
            Assert.Equal(SyntaxKind.PredefinedVectorType, funcDefStatement.ReturnType.Kind);
            Assert.Equal("float4", ((VectorTypeSyntax)funcDefStatement.ReturnType).TypeToken.Text);
            Assert.Equal(SyntaxKind.IdentifierDeclarationName, funcDefStatement.Name.Kind);
            Assert.Equal("GetTex", ((IdentifierDeclarationNameSyntax)funcDefStatement.Name).Name.Text);

            Assert.Equal(1, funcDefStatement.Body.Statements.Count);
            Assert.Equal(SyntaxKind.ReturnStatement, funcDefStatement.Body.Statements[0].Kind);
            var returnStatement = (ReturnStatementSyntax) funcDefStatement.Body.Statements[0];
            Assert.Equal(SyntaxKind.MethodInvocationExpression, returnStatement.Expression.Kind);
            var invocationExpr = (MethodInvocationExpressionSyntax) returnStatement.Expression;
            Assert.Equal(SyntaxKind.IdentifierName, invocationExpr.Target.Kind);
            var identifierNameExpr = (IdentifierNameSyntax) invocationExpr.Target;
            Assert.Equal("g_DiffuseTexture", identifierNameExpr.Name.Text);
            Assert.Equal("Sample", invocationExpr.Name.Text);
            Assert.Equal(2, invocationExpr.ArgumentList.Arguments.Count);
            Assert.Equal(SyntaxKind.IdentifierName, invocationExpr.ArgumentList.Arguments[0].Kind);
            Assert.Equal("DiffuseSampler", ((IdentifierNameSyntax)invocationExpr.ArgumentList.Arguments[0]).Name.Text);
            Assert.Equal(SyntaxKind.FieldAccessExpression, invocationExpr.ArgumentList.Arguments[1].Kind);
            Assert.Equal(SyntaxKind.FieldAccessExpression, ((FieldAccessExpressionSyntax)invocationExpr.ArgumentList.Arguments[1]).Expression.Kind);
            Assert.Equal(SyntaxKind.IdentifierName, ((FieldAccessExpressionSyntax)((FieldAccessExpressionSyntax)invocationExpr.ArgumentList.Arguments[1]).Expression).Expression.Kind);
            Assert.Equal("input", ((IdentifierNameSyntax) ((FieldAccessExpressionSyntax)((FieldAccessExpressionSyntax)invocationExpr.ArgumentList.Arguments[1]).Expression).Expression).Name.Text);
            Assert.Equal("texCoords", ((FieldAccessExpressionSyntax) ((FieldAccessExpressionSyntax)invocationExpr.ArgumentList.Arguments[1]).Expression).Name.Text);
            Assert.Equal("xy", ((FieldAccessExpressionSyntax)invocationExpr.ArgumentList.Arguments[1]).Name.Text);
        }

        [Fact]
        public void TestFunctionLikeDefineWithNestedPasteIncludingMacro()
        {
            const string text = @"
#define FOO
#define PASTE(x, y) x##y
#define PARAM(type, name) type name
PARAM(float, PASTE(bar, FOO)) = 1.0f;
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "FOO" },
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "PASTE" },
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "PARAM" });

            Assert.Equal(2, node.ChildNodes.Count);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[0]).Kind);

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.Equal(SyntaxKind.PredefinedScalarType, varDeclStatement.Declaration.Type.Kind);
            Assert.Equal(1, varDeclStatement.Declaration.Variables.Count);
            Assert.Equal("barFOO", varDeclStatement.Declaration.Variables[0].Identifier.Text);
            Assert.NotNull(varDeclStatement.Declaration.Variables[0].Initializer);
            Assert.Equal(SyntaxKind.EqualsValueClause, varDeclStatement.Declaration.Variables[0].Initializer.Kind);

            var equalsValueClause = (EqualsValueClauseSyntax)varDeclStatement.Declaration.Variables[0].Initializer;
            Assert.Equal(SyntaxKind.NumericLiteralExpression, equalsValueClause.Value.Kind);

            var floatLiteral = (LiteralExpressionSyntax)equalsValueClause.Value;
            Assert.Equal("1.0f", floatLiteral.Token.Text);
        }

        [Fact]
        public void TestTokenPastingOperator()
        {
            const string text = @"
#define FOO(a, b) a## b
float f = FOO(x, b);
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive });

            Assert.Equal(2, node.ChildNodes.Count);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[0]).Kind);

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.Equal(SyntaxKind.PredefinedScalarType, varDeclStatement.Declaration.Type.Kind);
            Assert.Equal(1, varDeclStatement.Declaration.Variables.Count);
            Assert.Equal("f", varDeclStatement.Declaration.Variables[0].Identifier.Text);
            Assert.NotNull(varDeclStatement.Declaration.Variables[0].Initializer);
            Assert.Equal(SyntaxKind.EqualsValueClause, varDeclStatement.Declaration.Variables[0].Initializer.Kind);

            var equalsValueClause = (EqualsValueClauseSyntax)varDeclStatement.Declaration.Variables[0].Initializer;
            Assert.Equal(SyntaxKind.IdentifierName, equalsValueClause.Value.Kind);

            var ident = (IdentifierNameSyntax)equalsValueClause.Value;
            Assert.Equal("xb", ident.Name.Text);
        }

        [Fact]
        public void TestStringificationOperator()
        {
            const string text = @"
#define TEX_COMP_FULL(format, packed) string Build_TexComp_DdsFormat = #format; string Build_TexComp_IsPacked = #packed;
Texture2D MyTex < TEX_COMP_FULL(dxt5, true) >;
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "TEX_COMP_FULL" });

            Assert.Equal(2, node.ChildNodes.Count);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[0]).Kind);

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.Equal(SyntaxKind.PredefinedObjectType, varDeclStatement.Declaration.Type.Kind);
            Assert.Equal(SyntaxKind.Texture2DKeyword, ((PredefinedObjectTypeSyntax) varDeclStatement.Declaration.Type).ObjectTypeToken.Kind);
            Assert.Equal(1, varDeclStatement.Declaration.Variables.Count);
            Assert.Equal("MyTex", varDeclStatement.Declaration.Variables[0].Identifier.Text);
            Assert.Equal(2, varDeclStatement.Declaration.Variables[0].Annotations.Annotations.Count);

            var annotation1 = varDeclStatement.Declaration.Variables[0].Annotations.Annotations[0];
            Assert.Equal(SyntaxKind.PredefinedScalarType, annotation1.Declaration.Type.Kind);
            Assert.Equal("Build_TexComp_DdsFormat", annotation1.Declaration.Variables[0].Identifier.Text);
            Assert.NotNull(annotation1.Declaration.Variables[0].Initializer);
            Assert.Equal(SyntaxKind.EqualsValueClause, annotation1.Declaration.Variables[0].Initializer.Kind);
            Assert.Equal(SyntaxKind.StringLiteralExpression, ((EqualsValueClauseSyntax) annotation1.Declaration.Variables[0].Initializer).Value.Kind);
            Assert.Equal(1, ((StringLiteralExpressionSyntax)((EqualsValueClauseSyntax)annotation1.Declaration.Variables[0].Initializer).Value).Tokens.Count);
            Assert.Equal("\"dxt5\"", ((StringLiteralExpressionSyntax) ((EqualsValueClauseSyntax)annotation1.Declaration.Variables[0].Initializer).Value).Tokens[0].Text);

            var annotation2 = varDeclStatement.Declaration.Variables[0].Annotations.Annotations[1];
            Assert.Equal(SyntaxKind.PredefinedScalarType, annotation2.Declaration.Type.Kind);
            Assert.Equal("Build_TexComp_IsPacked", annotation2.Declaration.Variables[0].Identifier.Text);
            Assert.NotNull(annotation2.Declaration.Variables[0].Initializer);
            Assert.Equal(SyntaxKind.EqualsValueClause, annotation2.Declaration.Variables[0].Initializer.Kind);
            Assert.Equal(SyntaxKind.StringLiteralExpression, ((EqualsValueClauseSyntax)annotation2.Declaration.Variables[0].Initializer).Value.Kind);
            Assert.Equal(1, ((StringLiteralExpressionSyntax)((EqualsValueClauseSyntax)annotation2.Declaration.Variables[0].Initializer).Value).Tokens.Count);
            Assert.Equal("\"true\"", ((StringLiteralExpressionSyntax)((EqualsValueClauseSyntax)annotation2.Declaration.Variables[0].Initializer).Value).Tokens[0].Text);

            Assert.Null(varDeclStatement.Declaration.Variables[0].Initializer);
        }

        [Fact]
        public void TestStringificationOperatorWithArgumentContainingMultipleTokens()
        {
            const string text = @"
#define FOO(value) #value""else""
string Bar = FOO(some/thing);
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "FOO" });

            Assert.Equal(2, node.ChildNodes.Count);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[0]).Kind);

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.Equal(SyntaxKind.PredefinedScalarType, varDeclStatement.Declaration.Type.Kind);
            Assert.Equal(1, ((ScalarTypeSyntax) varDeclStatement.Declaration.Type).TypeTokens.Count);
            Assert.Equal("string", ((ScalarTypeSyntax) varDeclStatement.Declaration.Type).TypeTokens[0].Text);
            Assert.Equal(1, varDeclStatement.Declaration.Variables.Count);
            Assert.Equal("Bar", varDeclStatement.Declaration.Variables[0].Identifier.Text);
            Assert.NotNull(varDeclStatement.Declaration.Variables[0].Initializer);
            Assert.Equal(SyntaxKind.EqualsValueClause, varDeclStatement.Declaration.Variables[0].Initializer.Kind);

            var initializerExpr = (StringLiteralExpressionSyntax) ((EqualsValueClauseSyntax) varDeclStatement.Declaration.Variables[0].Initializer).Value;
            Assert.Equal(2, initializerExpr.Tokens.Count);
            Assert.Equal("\"some/thing\"", initializerExpr.Tokens[0].Text);
            Assert.Equal("\"else\"", initializerExpr.Tokens[1].Text);
        }

        [Fact]
        public void TestMacroBodyContaining2D()
        {
            const string text = @"
#define TEX2D(name) Texture2D name ## 2D
TEX2D(MyTexture);
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "TEX2D" });

            Assert.Equal(2, node.ChildNodes.Count);
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, ((SyntaxNode) node.ChildNodes[0]).Kind);

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.Equal(SyntaxKind.PredefinedObjectType, varDeclStatement.Declaration.Type.Kind);
            Assert.Equal(1, varDeclStatement.Declaration.Variables.Count);
            Assert.Equal("MyTexture2D", varDeclStatement.Declaration.Variables[0].Identifier.Text);
            Assert.Null(varDeclStatement.Declaration.Variables[0].Initializer);
        }

        [Fact]
        public void TestNegBadDirectiveName()
        {
            const string text = @"#foo";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.DirectiveExpected);
            VerifyDirectives(node, SyntaxKind.BadDirectiveTrivia);
        }

        [Fact]
        public void TestNegBadDirectiveNoName()
        {
            const string text = @"#";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.DirectiveExpected);
            VerifyDirectives(node, SyntaxKind.BadDirectiveTrivia);
        }

        [Fact]
        public void TestNegBadDirectiveNameWithTrailingTokens()
        {
            const string text = @"#foo 2 _ a b";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.DirectiveExpected);
            VerifyDirectives(node, SyntaxKind.BadDirectiveTrivia);
        }

        [Fact]
        public void TestNegIfFalseWithEof()
        {
            const string text = @"#if false";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.EndIfDirectiveExpected);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue });
        }

        [Fact]
        public void TestNegIfWithNoCondition()
        {
            const string text = @"
#if
#endif";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            //VerifyErrorCode(node, DiagnosticId.InvalidPreprocessorExpression);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestNegIfTrueWithMissingParen()
        {
            const string text = @"
#if (true
#endif";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.TokenExpected);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.BranchTaken | NodeStatus.TrueValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestNegIfFalseWithMissingParen()
        {
            const string text = @"
#if (false
#endif";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.TokenExpected);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestIfFalseEndIf()
        {
            const string text = @"
#if 0
int a;
#endif";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestIfTrueEndIf()
        {
            const string text = @"
#if 1
int a;
#endif";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.BranchTaken | NodeStatus.TrueValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
            VerifyDeclarations(node, new DeclarationInfo { Kind = SyntaxKind.VariableDeclarationStatement, Text = "a" });
        }

        [Fact]
        public void TestIfTrueElseEndIf()
        {
            const string text = @"
#if 1
int a;
#else
float b;
#endif";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.BranchTaken | NodeStatus.TrueValue },
                new DirectiveInfo { Kind = SyntaxKind.ElseDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
            VerifyDeclarations(node, new DeclarationInfo { Kind = SyntaxKind.VariableDeclarationStatement, Text = "a" });
        }

        [Fact]
        public void TestIfTrueElifEndIf()
        {
            const string text = @"
#if 1
int a;
#elif 2
float b;
#endif";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.BranchTaken | NodeStatus.TrueValue },
                new DirectiveInfo { Kind = SyntaxKind.ElifDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.TrueValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
            VerifyDeclarations(node, new DeclarationInfo { Kind = SyntaxKind.VariableDeclarationStatement, Text = "a" });
        }

        [Fact]
        public void TestIfWithDefinedOnUndefined()
        {
            const string text = @"
#if defined(FOO)
int a;
#endif";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestIfWithDefinedOnDefined()
        {
            const string text = @"
#define FOO
#if defined(FOO)
int a;
#endif";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "FOO" },
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.BranchTaken | NodeStatus.TrueValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
            VerifyDeclarations(node, new DeclarationInfo { Kind = SyntaxKind.VariableDeclarationStatement, Text = "a" });
        }

        [Fact]
        public void TestIfWithDefinedWithoutParenthesesOnDefined()
        {
            const string text = @"
#define FOO
#if defined FOO
int a;
#endif";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "FOO" },
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.BranchTaken | NodeStatus.TrueValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
            VerifyDeclarations(node, new DeclarationInfo { Kind = SyntaxKind.VariableDeclarationStatement, Text = "a" });
        }

        [Fact]
        public void TestDirectiveAfterSingleLineComment()
        {
            const string text = @"
// foo #define BAR
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectives(node);
        }

        [Fact]
        public void TestSingleLineCommentAfterDirective()
        {
            const string text = @"
#define BAR 1 // foo
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "BAR" });
        }

        [Fact]
        public void TestSingleLineCommentInsideDirective()
        {
            const string text = @"
#if FALSE
    // A comment
#endif
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestIfFalseSingleLineCommentsAndDefine()
        {
            const string text = @"
#if FALSE
    // A comment
    #define int2 ivec2
    // Another comment
#endif
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue },
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsNotActive },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestIfTrueSingleLineCommentAndDefine()
        {
            const string text = @"
#if 1
    // A comment
    #define int2 ivec2
#endif
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.BranchTaken | NodeStatus.TrueValue },
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestLine()
        {
            const string text = @"
#line 3 ""a\path\to.hlsl""
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.LineDirectiveTrivia, Status = NodeStatus.IsActive, Number = 3, Text = @"a\path\to.hlsl" });
        }

        [Fact]
        public void HandlesPreprocessorDirectives()
        {
            // Act.
            var allTokens = LexAllTokens(@"
#if 1 == 2
                int a;
#else
                float b;
#endif");

            // Assert.
            Assert.Equal(4, allTokens.Count);
            Assert.Equal(SyntaxKind.FloatKeyword, allTokens[0].Kind);
            Assert.Equal(5, allTokens[0].LeadingTrivia.Length);
            Assert.Equal(SyntaxKind.EndOfLineTrivia, allTokens[0].LeadingTrivia[0].Kind);
            Assert.Equal(SyntaxKind.IfDirectiveTrivia, allTokens[0].LeadingTrivia[1].Kind);
            Assert.Equal(SyntaxKind.DisabledTextTrivia, allTokens[0].LeadingTrivia[2].Kind);
            Assert.Equal(SyntaxKind.ElseDirectiveTrivia, allTokens[0].LeadingTrivia[3].Kind);
            Assert.Equal(SyntaxKind.WhitespaceTrivia, allTokens[0].LeadingTrivia[4].Kind);
            Assert.Equal(1, allTokens[0].TrailingTrivia.Length);
            Assert.Equal(SyntaxKind.IdentifierToken, allTokens[1].Kind);
            Assert.Equal(SyntaxKind.SemiToken, allTokens[2].Kind);
            Assert.Equal(SyntaxKind.EndOfFileToken, allTokens[3].Kind);
            Assert.Equal(1, allTokens[3].LeadingTrivia.Length);
            Assert.Equal(SyntaxKind.EndIfDirectiveTrivia, allTokens[3].LeadingTrivia[0].Kind);
        }

        [Fact]
        public void TestInactiveInclude()
        {
            const string text = @"
#define FOO 1
#if FOO == 2
#include ""notused.hlsl""
#endif
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive },
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue },
                new DirectiveInfo { Kind = SyntaxKind.IncludeDirectiveTrivia, Status = NodeStatus.IsNotActive },
                new DirectiveInfo { Kind = SyntaxKind.EndIfDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestActiveInclude()
        {
            const string fooText = @"
#define DECL(n, v) int n = v
";
            const string text = @"
float foo;
#define FOO
#include <foo.hlsl>
DECL(i, 2);
float bar;
";
            var node = Parse(text, new InMemoryFileSystem(new Dictionary<string, string>
            {
                { "foo.hlsl", fooText }
            }));

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive },
                new DirectiveInfo { Kind = SyntaxKind.IncludeDirectiveTrivia, Status = NodeStatus.IsActive },
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestNegMissingInclude()
        {
            const string text = @"
#include <foo.hlsl>
";
            var node = Parse(text, new InMemoryFileSystem(new Dictionary<string, string>()));

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.IncludeNotFound);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IncludeDirectiveTrivia, Status = NodeStatus.IsActive });
        }

        [Fact]
        public void TestError()
        {
            const string text = @"
#error This is a compilation ""error""
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ErrorDirectiveTrivia, Status = NodeStatus.IsActive, Text = @"This is a compilation ""error""" });
        }

        [Fact]
        public void TestPragma()
        {
            const string text = @"
#pragma def(vs_4_0, c0, 0, 1, 2, 3)
#pragma message ""Debugging flag set""
#pragma warning(once: 1000)
#pragma pack_matrix(row_major)
#pragma something custom
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.PragmaDirectiveTrivia, Status = NodeStatus.IsActive, Text = @"def(vs_4_0, c0, 0, 1, 2, 3)" },
                new DirectiveInfo { Kind = SyntaxKind.PragmaDirectiveTrivia, Status = NodeStatus.IsActive, Text = @"message ""Debugging flag set""" },
                new DirectiveInfo { Kind = SyntaxKind.PragmaDirectiveTrivia, Status = NodeStatus.IsActive, Text = @"warning(once: 1000)" },
                new DirectiveInfo { Kind = SyntaxKind.PragmaDirectiveTrivia, Status = NodeStatus.IsActive, Text = @"pack_matrix(row_major)" },
                new DirectiveInfo { Kind = SyntaxKind.PragmaDirectiveTrivia, Status = NodeStatus.IsActive, Text = @"something custom" });
        }

        #region Test helpers

        private static IReadOnlyList<SyntaxToken> LexAllTokens(string text)
        {
            return SyntaxFactory.ParseAllTokens(new SourceFile(SourceText.From(text)));
        }

        private static CompilationUnitSyntax Parse(string text, IIncludeFileSystem fileSystem = null)
        {
            return SyntaxFactory.ParseCompilationUnit(new SourceFile(SourceText.From(text), "__Root__.hlsl"), fileSystem);
        }

        private static void TestRoundTripping(CompilationUnitSyntax node, string text, bool disallowErrors = true)
        {
            Assert.NotNull(node);

            var fullText = node.ToFullString();
            Assert.Equal(text, fullText);

            if (disallowErrors)
                Assert.Empty(node.GetDiagnostics());
            else
                Assert.NotEmpty(node.GetDiagnostics());
        }

        internal struct DirectiveInfo
        {
            public SyntaxKind Kind;
            public NodeStatus Status;
            public string Text;
            public int Number;
        }

        [Flags]
        public enum NodeStatus
        {
            None = 0,
            IsError = 1,
            IsWarning = 2,
            IsActive = 4,
            IsNotActive = 8, // used for #if etc.
            Unspecified = 8, // used for #def/und
            TrueValue = 16,
            Defined = 16, // used for #def/und
            FalseValue = 32,
            Undefined = 32, // used for #def/und
            BranchTaken = 64,
            NotBranchTaken = 128,
        }

        internal struct DeclarationInfo
        {
            public SyntaxKind Kind;
            public string Text;
        }

        private void VerifyDirectives(SyntaxNode node, params SyntaxKind[] expected)
        {
            var directives = node.GetDirectives().ToList();
            Assert.Equal(expected.Length, directives.Count);
            if (expected.Length == 0)
            {
                return;
            }

            List<SyntaxKind> actual = new List<SyntaxKind>();
            foreach (var dt in directives)
                actual.Add(dt.Kind);

            int idx = 0;
            foreach (var ek in expected)
            {
                // Assert.True(actualKinds.Contains(kind)); // no order 
                Assert.Equal(ek, actual[idx++]); // exact order
            }
        }

        private void VerifyDirectivesSpecial(SyntaxNode node, params DirectiveInfo[] expected)
        {
            var directives = node.GetDirectives().ToList();
            Assert.Equal(expected.Length, directives.Count);

            var actual = new List<SyntaxKind>();
            foreach (var dt in directives)
            {
                actual.Add(dt.Kind);
            }

            int idx = 0;
            foreach (var exp in expected)
            {
                Assert.Equal(exp.Kind, actual[idx]); // exact order

                // need to know what to expected here
                var dt = directives[idx++];

                if (NodeStatus.IsActive == (exp.Status & NodeStatus.IsActive))
                {
                    Assert.True(dt.IsActive);
                }
                else if (NodeStatus.IsNotActive == (exp.Status & NodeStatus.IsNotActive))
                {
                    Assert.False(dt.IsActive);
                }

                if (NodeStatus.BranchTaken == (exp.Status & NodeStatus.BranchTaken))
                {
                    Assert.True(((BranchingDirectiveTriviaSyntax)dt).BranchTaken);
                }
                else if (NodeStatus.NotBranchTaken == (exp.Status & NodeStatus.NotBranchTaken))
                {
                    Assert.False(((BranchingDirectiveTriviaSyntax)dt).BranchTaken);
                }

                if (NodeStatus.TrueValue == (exp.Status & NodeStatus.TrueValue))
                {
                    Assert.True(((ConditionalDirectiveTriviaSyntax)dt).ConditionValue);
                }
                else if (NodeStatus.FalseValue == (exp.Status & NodeStatus.FalseValue))
                {
                    Assert.False(((ConditionalDirectiveTriviaSyntax)dt).ConditionValue);
                }

                switch (exp.Kind)
                {
                    case SyntaxKind.ObjectLikeDefineDirectiveTrivia:
                        if (null != exp.Text)
                            Assert.Equal(exp.Text, ((ObjectLikeDefineDirectiveTriviaSyntax) dt).Name.Text); // Text
                        break;

                    case SyntaxKind.FunctionLikeDefineDirectiveTrivia:
                        if (null != exp.Text)
                            Assert.Equal(exp.Text, ((FunctionLikeDefineDirectiveTriviaSyntax) dt).Name.Text); // Text
                        break;

                    case SyntaxKind.LineDirectiveTrivia:
                        var ld = dt as LineDirectiveTriviaSyntax;

                        // default number = 0 - no number
                        if (exp.Number == -1)
                        {
                            Assert.Equal(SyntaxKind.LineKeyword, ld.LineKeyword.Kind);
                            Assert.Equal(SyntaxKind.DefaultKeyword, ld.Line.Kind);
                        }
                        else if (exp.Number == -2)
                        {
                            Assert.Equal(SyntaxKind.LineKeyword, ld.LineKeyword.Kind);
                            //Assert.Equal(SyntaxKind.HiddenKeyword, ld.Line.Kind);
                        }
                        else if (exp.Number == 0)
                        {
                            Assert.Equal(String.Empty, ld.Line.Text);
                        }
                        else if (exp.Number > 0)
                        {
                            Assert.Equal(exp.Number, ld.Line.Value); // Number
                            Assert.Equal(exp.Number, Int32.Parse(ld.Line.Text));
                        }

                        if (null == exp.Text)
                        {
                            Assert.Equal(SyntaxKind.None, ld.File.Kind);
                        }
                        else
                        {
                            Assert.NotEqual(SyntaxKind.None, ld.File.Kind);
                            Assert.Equal(exp.Text, ld.File.Value);
                        }

                        break;
                } // switch
            }
        }

        /// <summary>
        /// Not sure if this is good idea
        /// </summary>
        /// <param name="declarationInfo"></param>
        private void VerifyDeclarations(CompilationUnitSyntax node, params DeclarationInfo[] declarationInfo)
        {
            Assert.Equal(declarationInfo.Length, node.Declarations.Count);
            var actual = node.Declarations;
            int idx = 0;
            foreach (var exp in declarationInfo)
            {
                var mem = actual[idx++];
                Assert.Equal(exp.Kind, mem.Kind);
            }
        }

        private void VerifyErrorCode(SyntaxNode node, params DiagnosticId[] expected)
        {
            var actual = node.GetDiagnostics().Select(e => (DiagnosticId) e.Descriptor.Code).ToList();

            // no error
            if ((expected.Length == 0) && (actual.Count == 0))
            {
                return;
            }

            // Parser might give more errors than expected & that's fine
            Assert.True(actual.Count >= expected.Length);
            Assert.True(actual.Count <= int.MaxValue);

            // necessary?
            if (actual.Count < expected.Length)
                return;

            foreach (var i in expected)
                Assert.Contains(i, actual); // no order
        }

        #endregion
    }
}