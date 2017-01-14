using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Symbols;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Tests.Hlsl.Support;

namespace ShaderTools.Tests.Hlsl.Compilation
{
    [TestFixture]
    public class SemanticModelTests
    {
        [TestCaseSource(typeof(ShaderTestUtility), nameof(ShaderTestUtility.GetTestShaders))]
        public void CanGetSemanticModel(string testFile)
        {
            testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, testFile);

            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(sourceCode, testFile), fileSystem: new TestFileSystem());
            ShaderTestUtility.CheckForParseErrors(syntaxTree);

            // Get semantic model.
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            Assert.That(semanticModel, Is.Not.Null);

            foreach (var diagnostic in semanticModel.GetDiagnostics())
                Debug.WriteLine(diagnostic);

            Assert.That(semanticModel.GetDiagnostics().Count(x => x.Severity == DiagnosticSeverity.Error), Is.EqualTo(0));
        }

        [Test]
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
            Assert.That(structSymbol, Is.Not.Null);
            Assert.That(structSymbol.Name, Is.EqualTo("MyStruct"));
            Assert.That(structSymbol.Members, Has.Length.EqualTo(2));

            var functionSymbol = semanticModel.GetDeclaredSymbol(functionDefinition);
            Assert.That(functionSymbol, Is.Not.Null);
            Assert.That(functionSymbol.Name, Is.EqualTo("MyFunc"));
            Assert.That(functionSymbol.ReturnType, Is.EqualTo(IntrinsicTypes.Void));

            var variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclaration.Declaration.Variables[0]);
            Assert.That(variableSymbol, Is.Not.Null);
            Assert.That(variableSymbol.Name, Is.EqualTo("s"));
            Assert.That(variableSymbol.ValueType, Is.Not.Null);
            Assert.That(variableSymbol.ValueType, Is.EqualTo(structSymbol));

            var assignmentExpressionType = semanticModel.GetExpressionType(assignmentStatement.Expression);
            Assert.That(assignmentExpressionType, Is.Not.Null);
            Assert.That(assignmentExpressionType, Is.EqualTo(IntrinsicTypes.Float));
        }

        [Test]
        public void SemanticModelForTypedef()
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(@"
typedef float2 Point;
Point p;"));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            foreach (var diagnostic in semanticModel.GetDiagnostics())
                Debug.WriteLine(diagnostic);

            Assert.That(semanticModel.GetDiagnostics().Count(x => x.Severity == DiagnosticSeverity.Error), Is.EqualTo(0));

            var typedefStatement = (TypedefStatementSyntax) syntaxTree.Root.ChildNodes[0];
            var variableDeclaration = (VariableDeclarationStatementSyntax) syntaxTree.Root.ChildNodes[1];

            var typeAliasSymbol = semanticModel.GetDeclaredSymbol(typedefStatement.Declarators[0]);
            Assert.That(typeAliasSymbol, Is.Not.Null);
            Assert.That(typeAliasSymbol.Name, Is.EqualTo("Point"));

            var variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclaration.Declaration.Variables[0]);
            Assert.That(variableSymbol, Is.Not.Null);
            Assert.That(variableSymbol.Name, Is.EqualTo("p"));
            Assert.That(variableSymbol.ValueType, Is.Not.Null);
            Assert.That(variableSymbol.ValueType, Is.EqualTo(typeAliasSymbol));
        }
    }
}