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
        public void CanGetSemanticModel()
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
            var variableDeclaration = (VariableDeclarationStatementSyntax) syntaxTree.Root.ChildNodes[1];

            var structDefinitionSymbol = (StructSymbol) semanticModel.GetDeclaredSymbol((StructTypeSyntax) structDefinition.Type);
            Assert.That(structDefinitionSymbol.Name, Is.EqualTo("MyStruct"));
            Assert.That(structDefinitionSymbol.Members, Has.Length.EqualTo(2));

            var variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclaration.Declaration.Variables[0]);
            Assert.That(variableSymbol.ValueType, Is.EqualTo(structDefinitionSymbol));
        }
    }
}