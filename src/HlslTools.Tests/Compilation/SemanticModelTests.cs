using System.Diagnostics;
using System.IO;
using System.Linq;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Syntax;
using HlslTools.Tests.Support;
using HlslTools.Text;
using NUnit.Framework;

namespace HlslTools.Tests.Compilation
{
    [TestFixture]
    public class SemanticModelTests
    {
        [TestCaseSource(typeof(ShaderTestUtility), nameof(ShaderTestUtility.GetTestShaders))]
        public void CanGetSemanticModel(string testFile)
        {
            var sourceCode = File.ReadAllText(testFile);

            // Build syntax tree.
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(sourceCode), fileSystem: new TestFileSystem(testFile));
            ShaderTestUtility.CheckForParseErrors(syntaxTree);

            // Get semantic model.
            var compilation = new HlslTools.Compilation.Compilation(syntaxTree);
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
            var compilation = new HlslTools.Compilation.Compilation(syntaxTree);
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
    }
}