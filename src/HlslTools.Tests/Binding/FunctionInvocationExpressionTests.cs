using System.Diagnostics;
using System.Linq;
using HlslTools.Symbols;
using HlslTools.Symbols.Markup;
using HlslTools.Syntax;
using HlslTools.Text;
using NUnit.Framework;

namespace HlslTools.Tests.Binding
{
    [TestFixture]
    public class FunctionInvocationExpressionTests
    {
        [TestCase("int", "int")]
        [TestCase("uint", "int")]
        [TestCase("float", "float")]
        [TestCase("half", "#ambiguous")]
        [TestCase("half1", "#ambiguous")]
        [TestCase("float1", "float")]
        [TestCase("half2", "#ambiguous")]
        [TestCase("float2x1", "float2")]
        [TestCase("float1x2", "float2")]
        [TestCase("float1x3", "#ambiguous")]
        [TestCase("float3x1", "#ambiguous")]
        [TestCase("float2x3", "float")]
        [TestCase("float3x3", "float3x3")]
        [TestCase("half2x3", "#ambiguous")]
        [TestCase("int2x2", "int")]
        [TestCase("MyStruct", "#undeclared")]
        public void TestFunctionOverloadResolution1Arg(string type, string expectedMatchType)
        {
            var code = $@"
struct MyStruct {{}};

int foo(int x)      {{ return 1; }}
int foo(float x)    {{ return 2; }}
int foo(double x)   {{ return 3; }}
int foo(int2 x)     {{ return 4; }}
int foo(float2 x)   {{ return 5; }}
int foo(double2 x)  {{ return 6; }}
int foo(float3x3 x) {{ return 7; }}

void main()
{{
    foo(({type}) 0);
}}";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var syntaxTreeSource = syntaxTree.Root.ToFullString();
            Assert.AreEqual(code, syntaxTreeSource, $"Source should have been {code} but is {syntaxTreeSource}.");

            var expression = (FunctionInvocationExpressionSyntax) syntaxTree.Root.ChildNodes
                .OfType<FunctionDefinitionSyntax>()
                .Where(x => x.Name.GetName() == "main")
                .Select(x => ((ExpressionStatementSyntax) x.Body.Statements[0]).Expression)
                .First();

            var compilation = new HlslTools.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var combinedDiagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToList();

            foreach (var d in combinedDiagnostics)
                Debug.WriteLine(d);

            var invokedFunctionSymbol = (FunctionSymbol) semanticModel.GetSymbol(expression);
            var diagnostic = combinedDiagnostics.SingleOrDefault(x => x.Severity == Diagnostics.DiagnosticSeverity.Error);
            var result = diagnostic == null
                ? ExpressionTestUtility.GetExpressionTypeString(invokedFunctionSymbol.Parameters[0].ValueType)
                : ExpressionTestUtility.GetErrorString(diagnostic.DiagnosticId);

            Assert.AreEqual(expectedMatchType, result, $"Expression should have matched the function overload '{expectedMatchType}' but it actually matched '{result}'.");
        }

        [TestCase("float", "float", "float, float")]
        [TestCase("float", "half", "#ambiguous")]
        [TestCase("half", "half", "#ambiguous")]
        [TestCase("half2", "half", "#ambiguous")]
        [TestCase("float", "half", "#ambiguous")]
        [TestCase("double", "half", "double, float")]
        [TestCase("double", "double", "#ambiguous")]
        [TestCase("double", "bool", "#ambiguous")]
        [TestCase("double", "int", "double, int")]
        [TestCase("float2", "int", "float2, float")]
        [TestCase("float2", "half", "float2, float")]
        [TestCase("float2", "float", "float2, float")]
        [TestCase("float2", "double", "float2, float")]
        [TestCase("float3", "bool", "#ambiguous")]
        [TestCase("float3", "int", "float, int")]
        [TestCase("float3", "float", "#ambiguous")]
        [TestCase("int3", "float", "#ambiguous")]
        [TestCase("int3", "int", "#ambiguous")]
        [TestCase("float3", "double", "float, double")]
        [TestCase("float3x3", "bool", "float3x3, float")]
        [TestCase("float3x3", "half", "float3x3, float")]
        [TestCase("float3x3", "float", "float3x3, float")]
        [TestCase("float3x3", "double", "float3x3, float")]
        [TestCase("float", "int2", "float, int")]
        [TestCase("float", "half2", "#ambiguous")]
        [TestCase("float", "float2", "float, float")]
        [TestCase("float", "double2", "float, double")]
        [TestCase("float4x4", "float", "#ambiguous")]
        public void TestFunctionOverloadResolution2Args(string type1, string type2, string expectedMatchTypes)
        {
            var code = $@"
int foo(int x, float y)       {{ return 1; }}
int foo(float x, float y)     {{ return 2; }}
int foo(double x, float y)    {{ return 3; }}
int foo(float x, int y)       {{ return 4; }}
int foo(float x, double y)    {{ return 5; }}
int foo(double x, int y)      {{ return 6; }}
int foo(int2 x, float y)      {{ return 7; }}
int foo(float2 x, float y)    {{ return 8; }}
int foo(double2 x, float y)   {{ return 9; }}
int foo(float3x3 x, float y)  {{ return 10; }}

void main()
{{
    foo({ExpressionTestUtility.GetValue(type1)}, {ExpressionTestUtility.GetValue(type2)});
}}";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var syntaxTreeSource = syntaxTree.Root.ToFullString();
            Assert.AreEqual(code, syntaxTreeSource, $"Source should have been {code} but is {syntaxTreeSource}.");

            var expression = (FunctionInvocationExpressionSyntax) syntaxTree.Root.ChildNodes
                .OfType<FunctionDefinitionSyntax>()
                .Where(x => x.Name.GetName() == "main")
                .Select(x => ((ExpressionStatementSyntax) x.Body.Statements[0]).Expression)
                .First();

            var compilation = new HlslTools.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var combinedDiagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToList();

            foreach (var d in combinedDiagnostics)
                Debug.WriteLine(d);

            var invokedFunctionSymbol = (FunctionSymbol) semanticModel.GetSymbol(expression);

            var diagnostic = combinedDiagnostics.SingleOrDefault(x => x.Severity == Diagnostics.DiagnosticSeverity.Error);
            var result = diagnostic == null
                ? $"{SymbolMarkup.ForSymbol(invokedFunctionSymbol.Parameters[0].ValueType)}, {SymbolMarkup.ForSymbol(invokedFunctionSymbol.Parameters[1].ValueType)}"
                : ExpressionTestUtility.GetErrorString(diagnostic.DiagnosticId);

            Assert.AreEqual(expectedMatchTypes, result, $"Expression should have matched the function overload '{expectedMatchTypes}' but it actually matched '{result}'.");
        }

        [TestCase("min", "float", "float")]
        [TestCase("mul", "float4", "float4x4")]
        [TestCase("mul", "float3", "float4x4")]
        [TestCase("lerp", "float", "float", "float")]
        [TestCase("dot", "int", "uint")]
        public void TestIntrinsicFunctionOverloading(string function, params string[] types)
        {
            var arguments = string.Join(",", types.Select(x => $"({x}) 0"));
            var expressionCode = $"{function}({arguments})";
            var syntaxTree = SyntaxFactory.ParseExpression(expressionCode);
            var syntaxTreeSource = syntaxTree.Root.ToFullString();
            Assert.AreEqual(expressionCode, syntaxTreeSource, $"Source should have been {expressionCode} but is {syntaxTreeSource}.");

            var compilation = new HlslTools.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var combinedDiagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToList();

            foreach (var d in combinedDiagnostics)
                Debug.WriteLine(d);

            Assert.IsEmpty(combinedDiagnostics.Where(x => x.Severity == Diagnostics.DiagnosticSeverity.Error));
        }
    }
}