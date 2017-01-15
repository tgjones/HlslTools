using System.Linq;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Syntax;
using Xunit;

namespace ShaderTools.Tests.Hlsl.Binding
{
    public class BinaryExpressionTests
    {
        [Theory]
        [InlineData("|", "float", "int", "#inapplicable")]
        [InlineData("|", "bool", "float", "#inapplicable")]
        [InlineData("|", "bool", "int", "bool")]
        [InlineData("|", "int", "bool", "bool")]
        [InlineData("|", "int", "int", "int")]
        [InlineData("*", "float", "int", "float")]
        [InlineData("*", "int", "float", "float")]
        [InlineData("*", "int2", "float2", "float2")]
        [InlineData("*", "float2", "int2", "float2")]
        [InlineData("<<", "int", "uint", "uint")]
        public void TestBinaryOperatorTypeConversions(string opText, string leftText, string rightText, string expectedResult)
        {
            var left = ExpressionTestUtility.GetValue(leftText);
            var right = ExpressionTestUtility.GetValue(rightText);
            var source = $"{left} {opText} {right}";
            var syntaxTree = SyntaxFactory.ParseExpression(source);
            var syntaxTreeSource = syntaxTree.Root.ToString();
            Assert.Equal(source, syntaxTreeSource);

            var expression = (BinaryExpressionSyntax)syntaxTree.Root;
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var leftType = ExpressionTestUtility.GetExpressionTypeString(semanticModel.GetExpressionType(expression.Left));
            Assert.Equal(leftText, leftType);

            var rightType = ExpressionTestUtility.GetExpressionTypeString(semanticModel.GetExpressionType(expression.Right));
            Assert.Equal(rightText, rightType);

            var diagnostic = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).SingleOrDefault();
            var expressionType = semanticModel.GetExpressionType(expression);
            var result = diagnostic == null
                ? ExpressionTestUtility.GetExpressionTypeString(expressionType)
                : ExpressionTestUtility.GetErrorString((DiagnosticId) diagnostic.Descriptor.Code);

            Assert.Equal(expectedResult, result);
        }
    }
}