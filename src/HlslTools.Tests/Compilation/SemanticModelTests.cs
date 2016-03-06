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

MyStruct s;

float Scalar1;
int Scalar2;

float2 Vector1;
int4 Vector2;
vector<bool, 3> Vector3;

matrix Matrix1;
float1x2 Matrix2;
int2x3 Matrix3;
bool4x2 Matrix4;
matrix<uint, 3, 2> Matrix5;

float4x4 WorldViewProjection;

Texture2D Picture;
Texture2D<bool> PictureTyped;
SamplerState PictureSampler;
SamplerComparisonState PictureSamplerComparison;"));
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