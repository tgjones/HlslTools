using System;
using System.Linq;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Symbols.Markup;
using HlslTools.Syntax;
using NUnit.Framework;

namespace HlslTools.Tests.Binding
{
    [TestFixture]
    public class BinaryExpressionTests
    {
        [TestCase("*", "float", "int", "float")]
        [TestCase("*", "int", "float", "float")]
        [TestCase("*", "int2", "float2", "float2")]
        [TestCase("*", "float2", "int2", "float2")]
        public void TestBinaryOperatorTypeConversions(string opText, string leftText, string rightText, string expectedResult)
        {
            var left = GetValue(leftText);
            var right = GetValue(rightText);
            var source = $"{left} {opText} {right}";
            var syntaxTree = SyntaxFactory.ParseExpression(source);
            var syntaxTreeSource = syntaxTree.Root.ToString();
            if (syntaxTreeSource != source)
                Assert.Fail($"Source should have been {syntaxTreeSource} but is {source}");

            var expression = (BinaryExpressionSyntax)syntaxTree.Root;
            var compilation = new HlslTools.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var leftType = GetExpressionTypeString(semanticModel.GetExpressionType(expression.Left));
            if (leftText != leftType)
                Assert.Fail($"Left should be of type '{leftText}' but has type '{leftType}'");

            var rightType = GetExpressionTypeString(semanticModel.GetExpressionType(expression.Right));
            if (rightText != rightType)
                Assert.Fail($"Right should be of type '{rightText}' but has type '{rightType}'");

            var diagnostic = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).SingleOrDefault();
            var expressionType = semanticModel.GetExpressionType(expression);
            var result = diagnostic == null
                ? GetExpressionTypeString(expressionType)
                : GetErrorString(diagnostic.DiagnosticId);

            Assert.AreEqual(expectedResult, result, $"Expression {source} should have evaluated to '{expectedResult}' but was '{result}'");
        }

        private static string GetValue(string type)
        {
            return $"(({type}) 1)";
        }

        private static string GetExpressionTypeString(TypeSymbol type)
        {
            return SymbolMarkup.ForSymbol(type).ToString();
        }

        private static string GetErrorString(DiagnosticId diagnosticId)
        {
            switch (diagnosticId)
            {
                case DiagnosticId.CannotApplyUnaryOperator:
                case DiagnosticId.CannotApplyBinaryOperator:
                    return "#inapplicable";
                default:
                    throw new ArgumentOutOfRangeException(nameof(diagnosticId));
            }
        }
    }
}