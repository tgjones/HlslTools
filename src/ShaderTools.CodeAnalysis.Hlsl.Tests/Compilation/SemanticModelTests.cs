using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Tests.Support;
using ShaderTools.CodeAnalysis.Hlsl.Tests.TestSuite;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Testing;
using Xunit;
using Xunit.Abstractions;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Compilation
{
    public class SemanticModelTests
    {
        private readonly ITestOutputHelper _output;

        public SemanticModelTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [HlslTestSuiteData]
        public void CanGetSemanticModel(string testFile)
        {
            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(new SourceFile(SourceText.From(sourceCode), testFile), fileSystem: new TestFileSystem());
            SyntaxTreeUtility.CheckForParseErrors(syntaxTree);

            // Get semantic model.
            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            Assert.NotNull(semanticModel);

            foreach (var diagnostic in semanticModel.GetDiagnostics())
                _output.WriteLine(diagnostic.ToString());

            Assert.Equal(0, semanticModel.GetDiagnostics().Count(x => x.Severity == DiagnosticSeverity.Error));
        }

        [Fact]
        public void SemanticModelForStructAndFunction()
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(@"
struct MyStruct
{
    float a;
    int b;
};

void MyFunc()
{
    MyStruct s;
    s.a = 1.0;
}"));
            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var structDefinition = (TypeDeclarationStatementSyntax) syntaxTree.Root.ChildNodes[0];
            var functionDefinition = (FunctionDefinitionSyntax) syntaxTree.Root.ChildNodes[1];
            var variableDeclaration = (VariableDeclarationStatementSyntax) functionDefinition.Body.Statements[0];
            var assignmentStatement = (ExpressionStatementSyntax) functionDefinition.Body.Statements[1];

            var structSymbol = semanticModel.GetDeclaredSymbol((StructTypeSyntax) structDefinition.Type);
            Assert.NotNull(structSymbol);
            Assert.Equal("MyStruct", structSymbol.Name);
            Assert.Equal(2, structSymbol.Members.Length);

            var functionSymbol = semanticModel.GetDeclaredSymbol(functionDefinition);
            Assert.NotNull(functionSymbol);
            Assert.Equal("MyFunc", functionSymbol.Name);
            Assert.Equal(IntrinsicTypes.Void, functionSymbol.ReturnType);

            var variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclaration.Declaration.Variables[0]);
            Assert.NotNull(variableSymbol);
            Assert.Equal("s", variableSymbol.Name);
            Assert.NotNull(variableSymbol.ValueType);
            Assert.Equal(structSymbol, variableSymbol.ValueType);

            var assignmentExpressionType = semanticModel.GetExpressionType(assignmentStatement.Expression);
            Assert.NotNull(assignmentExpressionType);
            Assert.Equal(IntrinsicTypes.Float, assignmentExpressionType);
        }

        [Fact]
        public void SemanticModelForTypedef()
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(@"
typedef float2 Point;

void main() {
    Point p;
    p.x = 1;
}"));
            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            foreach (var diagnostic in semanticModel.GetDiagnostics())
                _output.WriteLine(diagnostic.ToString());

            Assert.Equal(0, semanticModel.GetDiagnostics().Count(x => x.Severity == DiagnosticSeverity.Error));

            var typedefStatement = (TypedefStatementSyntax) syntaxTree.Root.ChildNodes[0];
            var functionDefinition = (FunctionDefinitionSyntax) syntaxTree.Root.ChildNodes[1];

            var typeAliasSymbol = semanticModel.GetDeclaredSymbol(typedefStatement.Declarators[0]);
            Assert.NotNull(typeAliasSymbol);
            Assert.Equal("Point", typeAliasSymbol.Name);

            var variableDeclaration = (VariableDeclarationStatementSyntax) functionDefinition.Body.Statements[0];
            var variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclaration.Declaration.Variables[0]);
            Assert.NotNull(variableSymbol);
            Assert.Equal("p", variableSymbol.Name);
            Assert.NotNull(variableSymbol.ValueType);
            Assert.Equal(typeAliasSymbol, variableSymbol.ValueType);
        }

        [Fact]
        public void SemanticModelForTypedefStruct()
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(@"
typedef struct { int a; } MyStruct;
typedef struct { int a; } MyStruct2;"));
            var compilation = new Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            foreach (var diagnostic in semanticModel.GetDiagnostics())
                _output.WriteLine(diagnostic.ToString());

            Assert.Equal(0, semanticModel.GetDiagnostics().Count(x => x.Severity == DiagnosticSeverity.Error));

            var typedefStatement = (TypedefStatementSyntax)syntaxTree.Root.ChildNodes[0];

            var typeAliasSymbol = semanticModel.GetDeclaredSymbol(typedefStatement.Declarators[0]);
            Assert.NotNull(typeAliasSymbol);
            Assert.Equal("MyStruct", typeAliasSymbol.Name);

            var typedefStatement2 = (TypedefStatementSyntax)syntaxTree.Root.ChildNodes[1];

            var typeAliasSymbol2 = semanticModel.GetDeclaredSymbol(typedefStatement2.Declarators[0]);
            Assert.NotNull(typeAliasSymbol2);
            Assert.Equal("MyStruct2", typeAliasSymbol2.Name);
        }

        [Fact]
        public void SemanticModelForScalarSwizzle()
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(@"
float4 main() {
    return 1.xxxx + 2.0f.xxxx + 2.5.xxxx;
}"));
            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            foreach (var diagnostic in semanticModel.GetDiagnostics())
                _output.WriteLine(diagnostic.ToString());

            Assert.Equal(0, semanticModel.GetDiagnostics().Count(x => x.Severity == DiagnosticSeverity.Error));
        }
    }
}