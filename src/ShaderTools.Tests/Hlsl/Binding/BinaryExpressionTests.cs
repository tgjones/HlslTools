using System.Linq;
using NUnit.Framework;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Tests.Hlsl.Binding
{
    [TestFixture]
    public class BinaryExpressionTests
    {
        [TestCase("|", "float", "int", "#inapplicable")]
        [TestCase("|", "bool", "float", "#inapplicable")]
        [TestCase("|", "bool", "int", "bool")]
        [TestCase("|", "int", "bool", "bool")]
        [TestCase("|", "int", "int", "int")]
        [TestCase("*", "float", "int", "float")]
        [TestCase("*", "int", "float", "float")]
        [TestCase("*", "int2", "float2", "float2")]
        [TestCase("*", "float2", "int2", "float2")]
        [TestCase("<<", "int", "uint", "uint")]
        public void TestBinaryOperatorTypeConversions(string opText, string leftText, string rightText, string expectedResult)
        {
            var left = ExpressionTestUtility.GetValue(leftText);
            var right = ExpressionTestUtility.GetValue(rightText);
            var source = $"{left} {opText} {right}";
            var syntaxTree = SyntaxFactory.ParseExpression(source);
            var syntaxTreeSource = syntaxTree.Root.ToString();
            if (syntaxTreeSource != source)
                Assert.Fail($"Source should have been {syntaxTreeSource} but is {source}");

            var expression = (BinaryExpressionSyntax)syntaxTree.Root;
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var leftType = ExpressionTestUtility.GetExpressionTypeString(semanticModel.GetExpressionType(expression.Left));
            if (leftText != leftType)
                Assert.Fail($"Left should be of type '{leftText}' but has type '{leftType}'");

            var rightType = ExpressionTestUtility.GetExpressionTypeString(semanticModel.GetExpressionType(expression.Right));
            if (rightText != rightType)
                Assert.Fail($"Right should be of type '{rightText}' but has type '{rightType}'");

            var diagnostic = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).SingleOrDefault();
            var expressionType = semanticModel.GetExpressionType(expression);
            var result = diagnostic == null
                ? ExpressionTestUtility.GetExpressionTypeString(expressionType)
                : ExpressionTestUtility.GetErrorString((DiagnosticId) diagnostic.Descriptor.Code);

            Assert.AreEqual(expectedResult, result, $"Expression {source} should have evaluated to '{expectedResult}' but was '{result}'");
        }
    }
}