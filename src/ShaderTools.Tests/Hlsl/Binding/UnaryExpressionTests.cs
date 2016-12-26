using System.Linq;
using NUnit.Framework;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Tests.Hlsl.Binding
{
    [TestFixture]
    public class UnaryExpressionTests
    {
        [TestCase("-", "float", "float")]
        [TestCase("!", "float2", "bool2")]
        public void TestPrefixUnaryOperatorTypeConversions(string opText, string argumentText, string expectedResult)
        {
            var argument = ExpressionTestUtility.GetValue(argumentText);
            var source = $"{opText}{argument}";
            var syntaxTree = SyntaxFactory.ParseExpression(source);
            var syntaxTreeSource = syntaxTree.Root.ToString();
            if (syntaxTreeSource != source)
                Assert.Fail($"Source should have been {syntaxTreeSource} but is {source}");

            var expression = (PrefixUnaryExpressionSyntax)syntaxTree.Root;
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var operandType = ExpressionTestUtility.GetExpressionTypeString(semanticModel.GetExpressionType(expression.Operand));
            if (argumentText != operandType)
                Assert.Fail($"Operand should be of type '{argumentText}' but has type '{operandType}'");

            var diagnostic = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).SingleOrDefault();
            var expressionType = semanticModel.GetExpressionType(expression);
            var result = diagnostic == null
                ? ExpressionTestUtility.GetExpressionTypeString(expressionType)
                : ExpressionTestUtility.GetErrorString((DiagnosticId) diagnostic.Descriptor.Code);

            Assert.AreEqual(expectedResult, result, $"Expression {source} should have evaluated to '{expectedResult}' but was '{result}'");
        }
    }
}