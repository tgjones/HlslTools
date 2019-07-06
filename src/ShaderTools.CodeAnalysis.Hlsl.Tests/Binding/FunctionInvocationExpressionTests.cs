using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Binding
{
    public class FunctionInvocationExpressionTests
    {
        [Theory]
        [InlineData("int", "int")]
        [InlineData("uint", "int")]
        [InlineData("float", "float")]
        [InlineData("half", "#ambiguous")]
        [InlineData("half1", "#ambiguous")]
        [InlineData("float1", "float")]
        [InlineData("half2", "#ambiguous")]
        [InlineData("float2x1", "float2")]
        [InlineData("float1x2", "float2")]
        [InlineData("float1x3", "#ambiguous")]
        [InlineData("float3x1", "#ambiguous")]
        [InlineData("float2x3", "float")]
        [InlineData("float3x3", "float3x3")]
        [InlineData("half2x3", "#ambiguous")]
        [InlineData("int2x2", "int")]
        [InlineData("MyStruct", "#novalidoverload")]
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
            Assert.Equal(code, syntaxTreeSource);

            var expression = (FunctionInvocationExpressionSyntax) syntaxTree.Root.ChildNodes
                .OfType<FunctionDefinitionSyntax>()
                .Where(x => x.Name.GetUnqualifiedName().Name.Text == "main")
                .Select(x => ((ExpressionStatementSyntax) x.Body.Statements[0]).Expression)
                .First();

            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var combinedDiagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToList();

            foreach (var d in combinedDiagnostics)
                Debug.WriteLine(d);

            var invokedFunctionSymbol = (FunctionSymbol) semanticModel.GetSymbol(expression);
            var diagnostic = combinedDiagnostics.SingleOrDefault(x => x.Severity == DiagnosticSeverity.Error);
            var result = diagnostic == null
                ? ExpressionTestUtility.GetExpressionTypeString(invokedFunctionSymbol.Parameters[0].ValueType)
                : ExpressionTestUtility.GetErrorString((DiagnosticId) diagnostic.Descriptor.Code);

            Assert.Equal(expectedMatchType, result);
        }

        [Theory]
        [InlineData("float", "float", "float, float")]
        [InlineData("float", "half", "#ambiguous")]
        [InlineData("half", "half", "#ambiguous")]
        [InlineData("half2", "half", "#ambiguous")]
        [InlineData("double", "half", "double, float")]
        [InlineData("double", "double", "#ambiguous")]
        [InlineData("double", "bool", "#ambiguous")]
        [InlineData("double", "int", "double, int")]
        [InlineData("float2", "int", "float2, float")]
        [InlineData("float2", "half", "float2, float")]
        [InlineData("float2", "float", "float2, float")]
        [InlineData("float2", "double", "float2, float")]
        [InlineData("float3", "bool", "#ambiguous")]
        [InlineData("float3", "int", "float, int")]
        [InlineData("float3", "float", "#ambiguous")]
        [InlineData("int3", "float", "#ambiguous")]
        [InlineData("int3", "int", "#ambiguous")]
        [InlineData("float3", "double", "float, double")]
        [InlineData("float3x3", "bool", "float3x3, float")]
        [InlineData("float3x3", "half", "float3x3, float")]
        [InlineData("float3x3", "float", "float3x3, float")]
        [InlineData("float3x3", "double", "float3x3, float")]
        [InlineData("float", "int2", "float, int")]
        [InlineData("float", "half2", "#ambiguous")]
        [InlineData("float", "float2", "float, float")]
        [InlineData("float", "double2", "float, double")]
        [InlineData("float4x4", "float", "#ambiguous")]
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
            Assert.Equal(code, syntaxTreeSource);

            var expression = (FunctionInvocationExpressionSyntax) syntaxTree.Root.ChildNodes
                .OfType<FunctionDefinitionSyntax>()
                .Where(x => x.Name.GetUnqualifiedName().Name.Text == "main")
                .Select(x => ((ExpressionStatementSyntax) x.Body.Statements[0]).Expression)
                .First();

            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var combinedDiagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToList();

            foreach (var d in combinedDiagnostics)
                Debug.WriteLine(d);

            var invokedFunctionSymbol = (FunctionSymbol) semanticModel.GetSymbol(expression);

            var diagnostic = combinedDiagnostics.SingleOrDefault(x => x.Severity == DiagnosticSeverity.Error);
            var result = diagnostic == null
                ? $"{invokedFunctionSymbol.Parameters[0].ValueType.ToMarkup()}, {invokedFunctionSymbol.Parameters[1].ValueType.ToMarkup()}"
                : ExpressionTestUtility.GetErrorString((DiagnosticId) diagnostic.Descriptor.Code);

            Assert.Equal(expectedMatchTypes, result);
        }

        [Theory]
        [InlineData("min", "float", "float", "float, float")]
        [InlineData("mul", "float4", "float4x4", "float4, float4x4")]
        [InlineData("mul", "float3", "float4x4", "float3, float3x4")]
        [InlineData("mul", "float4", "float3x4", "float1x3, float3x4")]
        [InlineData("mul", "float1", "float3x4", "float, float3x4")]
        [InlineData("mul", "float4", "float4x3", "float4, float4x3")]
        [InlineData("mul", "float4x3", "float3x4", "float4x3, float3x4")]
        [InlineData("dot", "int", "uint", "int1, int1")]
        public void TestIntrinsicFunctionOverloading(string function, string type1, string type2, string expectedMatchTypes)
        {
            var expressionCode = $"{function}(({type1}) 0, ({type2}) 0)";
            var syntaxTree = SyntaxFactory.ParseExpression(expressionCode);
            var syntaxTreeSource = syntaxTree.Root.ToFullString();
            Assert.Equal(expressionCode, syntaxTreeSource);

            var expression = (ExpressionSyntax) syntaxTree.Root;

            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var combinedDiagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToList();

            foreach (var d in combinedDiagnostics)
                Debug.WriteLine(d);

            var invokedFunctionSymbol = (FunctionSymbol) semanticModel.GetSymbol(expression);

            var diagnostic = combinedDiagnostics.SingleOrDefault(x => x.Severity == DiagnosticSeverity.Error);
            var result = diagnostic == null
                ? $"{invokedFunctionSymbol.Parameters[0].ValueType.ToMarkup()}, {invokedFunctionSymbol.Parameters[1].ValueType.ToMarkup()}"
                : ExpressionTestUtility.GetErrorString((DiagnosticId) diagnostic.Descriptor.Code);

            Assert.Equal(expectedMatchTypes, result);
        }
    }
}