using System;
using System.Collections.Generic;
using System.Linq;
using HlslTools.Diagnostics;
using HlslTools.Syntax;
using HlslTools.Tests.Support;
using HlslTools.Text;
using NUnit.Framework;

namespace HlslTools.Tests.Parser
{
    [TestFixture]
    public class PreprocessorTests
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

            Assert.That(node.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(node.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.That(varDeclStatement.Declaration.Type.Kind, Is.EqualTo(SyntaxKind.PredefinedScalarType));
            Assert.That(varDeclStatement.Declaration.Variables, Has.Count.EqualTo(1));
            Assert.That(varDeclStatement.Declaration.Variables[0].Identifier.Text, Is.EqualTo("i"));
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer, Is.Not.Null);
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer.Kind, Is.EqualTo(SyntaxKind.EqualsValueClause));

            var equalsValueClause = (EqualsValueClauseSyntax)varDeclStatement.Declaration.Variables[0].Initializer;
            Assert.That(equalsValueClause.Value.Kind, Is.EqualTo(SyntaxKind.NumericLiteralExpression));

            var numericExpr = (LiteralExpressionSyntax) equalsValueClause.Value;
            Assert.That(numericExpr.Token.Text, Is.EqualTo("1024"));
        }

        [Test]
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

            Assert.That(node.ChildNodes, Has.Count.EqualTo(3));
            Assert.That(node.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));

            var varDeclStatement = (VariableDeclarationStatementSyntax) node.ChildNodes[0];
            Assert.That(varDeclStatement.Declaration.Type.Kind, Is.EqualTo(SyntaxKind.PredefinedScalarType));
            Assert.That(varDeclStatement.Declaration.Variables, Has.Count.EqualTo(1));
            Assert.That(varDeclStatement.Declaration.Variables[0].Identifier.Text, Is.EqualTo("f"));
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer, Is.Not.Null);
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer.Kind, Is.EqualTo(SyntaxKind.EqualsValueClause));

            var equalsValueClause = (EqualsValueClauseSyntax) varDeclStatement.Declaration.Variables[0].Initializer;
            Assert.That(equalsValueClause.Value.Kind, Is.EqualTo(SyntaxKind.AddExpression));

            var addExpr = (BinaryExpressionSyntax) equalsValueClause.Value;
            Assert.That(addExpr.Left.Kind, Is.EqualTo(SyntaxKind.NumericLiteralExpression));
            Assert.That(((LiteralExpressionSyntax) addExpr.Left).Token.Text, Is.EqualTo("1"));
            Assert.That(addExpr.OperatorToken.Kind, Is.EqualTo(SyntaxKind.PlusToken));
            Assert.That(addExpr.Right.Kind, Is.EqualTo(SyntaxKind.NumericLiteralExpression));
            Assert.That(((LiteralExpressionSyntax)addExpr.Right).Token.Text, Is.EqualTo("2"));

            Assert.That(node.ChildNodes[1].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));
        }

        [Test]
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

            Assert.That(node.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(node.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.That(varDeclStatement.Declaration.Type.Kind, Is.EqualTo(SyntaxKind.PredefinedScalarType));
            Assert.That(varDeclStatement.Declaration.Variables, Has.Count.EqualTo(1));
            Assert.That(varDeclStatement.Declaration.Variables[0].Identifier.Text, Is.EqualTo("i"));
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer, Is.Not.Null);
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer.Kind, Is.EqualTo(SyntaxKind.EqualsValueClause));

            var equalsValueClause = (EqualsValueClauseSyntax)varDeclStatement.Declaration.Variables[0].Initializer;
            Assert.That(equalsValueClause.Value.Kind, Is.EqualTo(SyntaxKind.AddExpression));

            var addExpr = (BinaryExpressionSyntax)equalsValueClause.Value;
            Assert.That(addExpr.Left.Kind, Is.EqualTo(SyntaxKind.MultiplyExpression));

            var mulExpr = (BinaryExpressionSyntax) addExpr.Left;
            Assert.That(mulExpr.Left.Kind, Is.EqualTo(SyntaxKind.NumericLiteralExpression));
            Assert.That(mulExpr.OperatorToken.Kind, Is.EqualTo(SyntaxKind.AsteriskToken));
            Assert.That(mulExpr.Right.Kind, Is.EqualTo(SyntaxKind.NumericLiteralExpression));

            Assert.That(addExpr.OperatorToken.Kind, Is.EqualTo(SyntaxKind.PlusToken));
            Assert.That(addExpr.Right.Kind, Is.EqualTo(SyntaxKind.NumericLiteralExpression));
        }

        [Test]
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

            Assert.That(node.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(node.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.That(varDeclStatement.Declaration.Type.Kind, Is.EqualTo(SyntaxKind.PredefinedScalarType));
            Assert.That(varDeclStatement.Declaration.Variables, Has.Count.EqualTo(1));
            Assert.That(varDeclStatement.Declaration.Variables[0].Identifier.Text, Is.EqualTo("i"));
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer, Is.Not.Null);
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer.Kind, Is.EqualTo(SyntaxKind.EqualsValueClause));

            var equalsValueClause = (EqualsValueClauseSyntax)varDeclStatement.Declaration.Variables[0].Initializer;
            Assert.That(equalsValueClause.Value.Kind, Is.EqualTo(SyntaxKind.IdentifierName));

            var identName = (IdentifierNameSyntax)equalsValueClause.Value;
            Assert.That(identName.Name.Text, Is.EqualTo("MULTIPLY"));
        }

        [Test]
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

        [Test]
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

            Assert.That(node.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(node.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.That(varDeclStatement.Declaration.Type.Kind, Is.EqualTo(SyntaxKind.PredefinedScalarType));
            Assert.That(varDeclStatement.Declaration.Variables, Has.Count.EqualTo(1));
            Assert.That(varDeclStatement.Declaration.Variables[0].Identifier.Text, Is.EqualTo("barbaz"));
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer, Is.Not.Null);
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer.Kind, Is.EqualTo(SyntaxKind.EqualsValueClause));
        }

        [Test]
        public void TestNegFunctionLikeDefineWithNestedPasteIncludingMacro()
        {
            const string text = @"
#define FOO
#define PASTE(x, y) x##y
#define PARAM(type, name) type name
PARAM(float, PASTE(bar, FOO)) = 1.0f;
";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.TokenExpected, DiagnosticId.TokenUnexpected);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.ObjectLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "FOO" },
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "PASTE" },
                new DirectiveInfo { Kind = SyntaxKind.FunctionLikeDefineDirectiveTrivia, Status = NodeStatus.IsActive, Text = "PARAM" });
        }

        [Test]
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

            Assert.That(node.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(node.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.That(varDeclStatement.Declaration.Type.Kind, Is.EqualTo(SyntaxKind.PredefinedScalarType));
            Assert.That(varDeclStatement.Declaration.Variables, Has.Count.EqualTo(1));
            Assert.That(varDeclStatement.Declaration.Variables[0].Identifier.Text, Is.EqualTo("f"));
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer, Is.Not.Null);
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer.Kind, Is.EqualTo(SyntaxKind.EqualsValueClause));

            var equalsValueClause = (EqualsValueClauseSyntax)varDeclStatement.Declaration.Variables[0].Initializer;
            Assert.That(equalsValueClause.Value.Kind, Is.EqualTo(SyntaxKind.IdentifierName));

            var ident = (IdentifierNameSyntax)equalsValueClause.Value;
            Assert.That(ident.Name.Text, Is.EqualTo("xb"));
        }

        [Test]
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

            Assert.That(node.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(node.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.That(varDeclStatement.Declaration.Type.Kind, Is.EqualTo(SyntaxKind.PredefinedObjectType));
            Assert.That(((PredefinedObjectTypeSyntax) varDeclStatement.Declaration.Type).ObjectTypeToken.Kind, Is.EqualTo(SyntaxKind.Texture2DKeyword));
            Assert.That(varDeclStatement.Declaration.Variables, Has.Count.EqualTo(1));
            Assert.That(varDeclStatement.Declaration.Variables[0].Identifier.Text, Is.EqualTo("MyTex"));
            Assert.That(varDeclStatement.Declaration.Variables[0].Annotations.Annotations, Has.Count.EqualTo(2));

            var annotation1 = varDeclStatement.Declaration.Variables[0].Annotations.Annotations[0];
            Assert.That(annotation1.Declaration.Type.Kind, Is.EqualTo(SyntaxKind.IdentifierName));
            Assert.That(annotation1.Declaration.Variables[0].Identifier.Text, Is.EqualTo("Build_TexComp_DdsFormat"));
            Assert.That(annotation1.Declaration.Variables[0].Initializer, Is.Not.Null);
            Assert.That(annotation1.Declaration.Variables[0].Initializer.Kind, Is.EqualTo(SyntaxKind.EqualsValueClause));
            Assert.That(((EqualsValueClauseSyntax) annotation1.Declaration.Variables[0].Initializer).Value.Kind, Is.EqualTo(SyntaxKind.StringLiteralExpression));
            Assert.That(((StringLiteralExpressionSyntax)((EqualsValueClauseSyntax)annotation1.Declaration.Variables[0].Initializer).Value).Tokens, Has.Count.EqualTo(1));
            Assert.That(((StringLiteralExpressionSyntax) ((EqualsValueClauseSyntax)annotation1.Declaration.Variables[0].Initializer).Value).Tokens[0].Text, Is.EqualTo("\"dxt5\""));

            var annotation2 = varDeclStatement.Declaration.Variables[0].Annotations.Annotations[1];
            Assert.That(annotation2.Declaration.Type.Kind, Is.EqualTo(SyntaxKind.IdentifierName));
            Assert.That(annotation2.Declaration.Variables[0].Identifier.Text, Is.EqualTo("Build_TexComp_IsPacked"));
            Assert.That(annotation2.Declaration.Variables[0].Initializer, Is.Not.Null);
            Assert.That(annotation2.Declaration.Variables[0].Initializer.Kind, Is.EqualTo(SyntaxKind.EqualsValueClause));
            Assert.That(((EqualsValueClauseSyntax)annotation2.Declaration.Variables[0].Initializer).Value.Kind, Is.EqualTo(SyntaxKind.StringLiteralExpression));
            Assert.That(((StringLiteralExpressionSyntax)((EqualsValueClauseSyntax)annotation2.Declaration.Variables[0].Initializer).Value).Tokens, Has.Count.EqualTo(1));
            Assert.That(((StringLiteralExpressionSyntax)((EqualsValueClauseSyntax)annotation2.Declaration.Variables[0].Initializer).Value).Tokens[0].Text, Is.EqualTo("\"true\""));

            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer, Is.Null);
        }

        [Test]
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

            Assert.That(node.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(node.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.That(varDeclStatement.Declaration.Type.Kind, Is.EqualTo(SyntaxKind.IdentifierName));
            Assert.That(((IdentifierNameSyntax)varDeclStatement.Declaration.Type).Name.Text, Is.EqualTo("string"));
            Assert.That(varDeclStatement.Declaration.Variables, Has.Count.EqualTo(1));
            Assert.That(varDeclStatement.Declaration.Variables[0].Identifier.Text, Is.EqualTo("Bar"));
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer, Is.Not.Null);
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer.Kind, Is.EqualTo(SyntaxKind.EqualsValueClause));

            var initializerExpr = (StringLiteralExpressionSyntax) ((EqualsValueClauseSyntax) varDeclStatement.Declaration.Variables[0].Initializer).Value;
            Assert.That(initializerExpr.Tokens, Has.Count.EqualTo(2));
            Assert.That(initializerExpr.Tokens[0].Text, Is.EqualTo("\"some/thing\""));
            Assert.That(initializerExpr.Tokens[1].Text, Is.EqualTo("\"else\""));
        }

        [Test]
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

            Assert.That(node.ChildNodes, Has.Count.EqualTo(2));
            Assert.That(node.ChildNodes[0].Kind, Is.EqualTo(SyntaxKind.VariableDeclarationStatement));

            var varDeclStatement = (VariableDeclarationStatementSyntax)node.ChildNodes[0];
            Assert.That(varDeclStatement.Declaration.Type.Kind, Is.EqualTo(SyntaxKind.PredefinedObjectType));
            Assert.That(varDeclStatement.Declaration.Variables, Has.Count.EqualTo(1));
            Assert.That(varDeclStatement.Declaration.Variables[0].Identifier.Text, Is.EqualTo("MyTexture2D"));
            Assert.That(varDeclStatement.Declaration.Variables[0].Initializer, Is.Null);
        }

        [Test]
        public void TestNegBadDirectiveName()
        {
            const string text = @"#foo";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.DirectiveExpected);
            VerifyDirectives(node, SyntaxKind.BadDirectiveTrivia);
        }

        [Test]
        public void TestNegBadDirectiveNoName()
        {
            const string text = @"#";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.DirectiveExpected);
            VerifyDirectives(node, SyntaxKind.BadDirectiveTrivia);
        }

        [Test]
        public void TestNegBadDirectiveNameWithTrailingTokens()
        {
            const string text = @"#foo 2 _ a b";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.DirectiveExpected);
            VerifyDirectives(node, SyntaxKind.BadDirectiveTrivia);
        }

        [Test]
        public void TestNegIfFalseWithEof()
        {
            const string text = @"#if false";
            var node = Parse(text);

            TestRoundTripping(node, text, false);
            VerifyErrorCode(node, DiagnosticId.EndIfDirectiveExpected);
            VerifyDirectivesSpecial(node,
                new DirectiveInfo { Kind = SyntaxKind.IfDirectiveTrivia, Status = NodeStatus.IsActive | NodeStatus.NotBranchTaken | NodeStatus.FalseValue });
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void TestDirectiveAfterSingleLineComment()
        {
            const string text = @"
// foo #define BAR
";
            var node = Parse(text);

            TestRoundTripping(node, text);
            VerifyDirectives(node);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
            Assert.That(allTokens, Has.Count.EqualTo(4));
            Assert.That(allTokens[0].Kind, Is.EqualTo(SyntaxKind.FloatKeyword));
            Assert.That(allTokens[0].LeadingTrivia, Has.Length.EqualTo(5));
            Assert.That(allTokens[0].LeadingTrivia[0].Kind, Is.EqualTo(SyntaxKind.EndOfLineTrivia));
            Assert.That(allTokens[0].LeadingTrivia[1].Kind, Is.EqualTo(SyntaxKind.IfDirectiveTrivia));
            Assert.That(allTokens[0].LeadingTrivia[2].Kind, Is.EqualTo(SyntaxKind.DisabledTextTrivia));
            Assert.That(allTokens[0].LeadingTrivia[3].Kind, Is.EqualTo(SyntaxKind.ElseDirectiveTrivia));
            Assert.That(allTokens[0].LeadingTrivia[4].Kind, Is.EqualTo(SyntaxKind.WhitespaceTrivia));
            Assert.That(allTokens[0].TrailingTrivia, Has.Length.EqualTo(1));
            Assert.That(allTokens[1].Kind, Is.EqualTo(SyntaxKind.IdentifierToken));
            Assert.That(allTokens[2].Kind, Is.EqualTo(SyntaxKind.SemiToken));
            Assert.That(allTokens[3].Kind, Is.EqualTo(SyntaxKind.EndOfFileToken));
            Assert.That(allTokens[3].LeadingTrivia, Has.Length.EqualTo(1));
            Assert.That(allTokens[3].LeadingTrivia[0].Kind, Is.EqualTo(SyntaxKind.EndIfDirectiveTrivia));
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
            return SyntaxFactory.ParseAllTokens(SourceText.From(text));
        }

        private static CompilationUnitSyntax Parse(string text, IIncludeFileSystem fileSystem = null)
        {
            return SyntaxFactory.ParseCompilationUnit(text, fileSystem);
        }

        private static void TestRoundTripping(CompilationUnitSyntax node, string text, bool disallowErrors = true)
        {
            Assert.That(node, Is.Not.Null);
            var fullText = node.ToFullString();
            Assert.That(fullText, Is.EqualTo(text));

            if (disallowErrors)
                Assert.That(node.GetDiagnostics(), Is.Empty);
            else
                Assert.That(node.GetDiagnostics(), Is.Not.Empty);
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
            Assert.AreEqual(expected.Length, directives.Count);
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
                Assert.AreEqual(ek, actual[idx++]); // exact order
            }
        }

        private void VerifyDirectivesSpecial(SyntaxNode node, params DirectiveInfo[] expected)
        {
            var directives = node.GetDirectives().ToList();
            Assert.AreEqual(expected.Length, directives.Count);

            var actual = new List<SyntaxKind>();
            foreach (var dt in directives)
            {
                actual.Add(dt.Kind);
            }

            int idx = 0;
            foreach (var exp in expected)
            {
                Assert.AreEqual(exp.Kind, actual[idx]); // exact order

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
                            Assert.AreEqual(exp.Text, ((ObjectLikeDefineDirectiveTriviaSyntax) dt).Name.Text); // Text
                        break;

                    case SyntaxKind.FunctionLikeDefineDirectiveTrivia:
                        if (null != exp.Text)
                            Assert.AreEqual(exp.Text, ((FunctionLikeDefineDirectiveTriviaSyntax) dt).Name.Text); // Text
                        break;

                    case SyntaxKind.LineDirectiveTrivia:
                        var ld = dt as LineDirectiveTriviaSyntax;

                        // default number = 0 - no number
                        if (exp.Number == -1)
                        {
                            Assert.AreEqual(SyntaxKind.LineKeyword, ld.LineKeyword.Kind);
                            Assert.AreEqual(SyntaxKind.DefaultKeyword, ld.Line.Kind);
                        }
                        else if (exp.Number == -2)
                        {
                            Assert.AreEqual(SyntaxKind.LineKeyword, ld.LineKeyword.Kind);
                            //Assert.AreEqual(SyntaxKind.HiddenKeyword, ld.Line.Kind);
                        }
                        else if (exp.Number == 0)
                        {
                            Assert.AreEqual(String.Empty, ld.Line.Text);
                        }
                        else if (exp.Number > 0)
                        {
                            Assert.AreEqual(exp.Number, ld.Line.Value); // Number
                            Assert.AreEqual(exp.Number, Int32.Parse(ld.Line.Text));
                        }

                        if (null == exp.Text)
                        {
                            Assert.AreEqual(SyntaxKind.None, ld.File.Kind);
                        }
                        else
                        {
                            Assert.AreNotEqual(SyntaxKind.None, ld.File.Kind);
                            Assert.AreEqual(exp.Text, ld.File.Value);
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
            Assert.AreEqual(declarationInfo.Length, node.Declarations.Count);
            var actual = node.Declarations;
            int idx = 0;
            foreach (var exp in declarationInfo)
            {
                var mem = actual[idx++];
                Assert.AreEqual(exp.Kind, mem.Kind);
            }
        }

        private void VerifyErrorCode(SyntaxNode node, params DiagnosticId[] expected)
        {
            var actual = node.GetDiagnostics().Select(e => e.DiagnosticId).ToList();

            // no error
            if ((expected.Length == 0) && (actual.Count == 0))
            {
                return;
            }

            // Parser might give more errors than expected & that's fine
            Assert.That(actual.Count, Is.GreaterThanOrEqualTo(expected.Length));
            Assert.That(actual.Count, Is.LessThanOrEqualTo(int.MaxValue));

            // necessary?
            if (actual.Count < expected.Length)
                return;

            foreach (var i in expected)
                Assert.Contains(i, actual); // no order
        }

        #endregion
    }
}