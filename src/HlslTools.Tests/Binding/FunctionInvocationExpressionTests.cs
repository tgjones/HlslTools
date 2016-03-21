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
        [TestCase("half2x3", "float")]
        [TestCase("int2x2", "int")]
        [TestCase("MyStruct", "#undeclared")]
        public void TestFunctionOverloadResolution1Arg(string type, string expectedMatchType)
        {
            var code = $@"
struct MyStruct {{}};

void foo(int x);
void foo(float x);
void foo(double x);
void foo(int2 x);
void foo(float2 x);
void foo(double2 x);
void foo(float3x3 x);

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

            var invokedFunctionSymbol = (FunctionSymbol) semanticModel.GetSymbol(expression);
            var matchType = (invokedFunctionSymbol != null)
                ? SymbolMarkup.ForSymbol(invokedFunctionSymbol.Parameters[0].ValueType).ToString()
                : "#undeclared";
            Assert.AreEqual(expectedMatchType, matchType, $"Expression should have matched the function overload taking a '{expectedMatchType}' but it actually matched '{matchType}'.");
        }

        [TestCase("float", "float", "float, float")]
        [TestCase("float", "half", "float, float")]
        [TestCase("half2", "half", "#ambiguous")]
        public void TestFunctionOverloadResolution2Args(string type1, string type2, string expectedMatchTypes)
        {
            var code = $@"void foo(int x, float y);
void foo(float x, float y);
void foo(double x, float y);
void foo(int2 x, float y);
void foo(float2 x, float y);
void foo(double2 x, float y);
void foo(float3x3 x, float y);

void main()
{{
    foo(({type1}) 0, ({type2}) 0);
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

            var invokedFunctionSymbol = (FunctionSymbol) semanticModel.GetSymbol(expression);
            var matchTypes = (invokedFunctionSymbol != null)
                ? $"{SymbolMarkup.ForSymbol(invokedFunctionSymbol.Parameters[0].ValueType)}, {SymbolMarkup.ForSymbol(invokedFunctionSymbol.Parameters[1].ValueType)}"
                : "#undeclared";
            Assert.AreEqual(expectedMatchTypes, matchTypes, $"Expression should have matched the function overload taking '{expectedMatchTypes}' but it actually matched '{matchTypes}'.");
        }
    }
}