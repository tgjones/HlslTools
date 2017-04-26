using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Binding
{
    public class UnaryExpressionTests
    {
        [Theory]
        [InlineData("-", "float", "float")]
        [InlineData("!", "float2", "bool2")]
        public void TestPrefixUnaryOperatorTypeConversions(string opText, string argumentText, string expectedResult)
        {
            var argument = ExpressionTestUtility.GetValue(argumentText);
            var source = $"{opText}{argument}";
            var syntaxTree = SyntaxFactory.ParseExpression(source);
            var syntaxTreeSource = syntaxTree.Root.ToString();
            Assert.Equal(source, syntaxTreeSource);

            var expression = (PrefixUnaryExpressionSyntax)syntaxTree.Root;
            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var operandType = ExpressionTestUtility.GetExpressionTypeString(semanticModel.GetExpressionType(expression.Operand));
            Assert.Equal(argumentText, operandType);

            var diagnostic = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).SingleOrDefault();
            var expressionType = semanticModel.GetExpressionType(expression);
            var result = diagnostic == null
                ? ExpressionTestUtility.GetExpressionTypeString(expressionType)
                : ExpressionTestUtility.GetErrorString((DiagnosticId) diagnostic.Descriptor.Code);

            Assert.Equal(expectedResult, result);
        }
    }
}