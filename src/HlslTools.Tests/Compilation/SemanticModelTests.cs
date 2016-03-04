using HlslTools.Symbols;
using HlslTools.Syntax;
using HlslTools.Text;
using NUnit.Framework;

namespace HlslTools.Tests.Compilation
{
    [TestFixture]
    public class SemanticModelTests
    {
        [Test]
        public void GetSemanticModel()
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(@"
struct MyStruct
{
    float a;
    int b;
};

MyStruct s;"));
            var compilation = new HlslTools.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var structDefinition = (TypeDeclarationStatementSyntax) syntaxTree.Root.ChildNodes[0];
            var variableDeclaration = (VariableDeclarationStatementSyntax) syntaxTree.Root.ChildNodes[1];

            var structDefinitionSymbol = (StructSymbol) semanticModel.GetSymbol(structDefinition);
            Assert.That(structDefinitionSymbol.Name, Is.EqualTo("MyStruct"));
            Assert.That(structDefinitionSymbol.Members, Has.Length.EqualTo(2));

            var variableSymbol = (GlobalVariableSymbol) semanticModel.GetSymbol(variableDeclaration.Declaration.Variables[0]);
            Assert.That(variableSymbol.ValueType, Is.EqualTo(structDefinitionSymbol));
        }
    }
}