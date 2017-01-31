using System.Diagnostics;
using System.IO;
using System.Linq;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Symbols;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Testing;
using ShaderTools.Testing.TestResources.Hlsl;
using ShaderTools.Tests.Hlsl.Support;
using Xunit;
using Xunit.Abstractions;

namespace ShaderTools.Tests.Hlsl.Compilation
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
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(sourceCode, testFile), fileSystem: new TestFileSystem());
            SyntaxTreeUtility.CheckForParseErrors(syntaxTree);

            // Get semantic model.
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
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
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
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
Point p;"));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            foreach (var diagnostic in semanticModel.GetDiagnostics())
                Debug.WriteLine(diagnostic);

            Assert.Equal(0, semanticModel.GetDiagnostics().Count(x => x.Severity == DiagnosticSeverity.Error));

            var typedefStatement = (TypedefStatementSyntax) syntaxTree.Root.ChildNodes[0];
            var variableDeclaration = (VariableDeclarationStatementSyntax) syntaxTree.Root.ChildNodes[1];

            var typeAliasSymbol = semanticModel.GetDeclaredSymbol(typedefStatement.Declarators[0]);
            Assert.NotNull(typeAliasSymbol);
            Assert.Equal("Point", typeAliasSymbol.Name);

            var variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclaration.Declaration.Variables[0]);
            Assert.NotNull(variableSymbol);
            Assert.Equal("p", variableSymbol.Name);
            Assert.NotNull(variableSymbol.ValueType);
            Assert.Equal(typeAliasSymbol, variableSymbol.ValueType);
        }
    }
}