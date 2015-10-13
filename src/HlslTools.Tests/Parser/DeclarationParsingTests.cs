using System.Linq;
using HlslTools.Syntax;
using NUnit.Framework;

namespace HlslTools.Tests.Parser
{
    [TestFixture]
    public class DeclarationParsingTests
    {
        [TestCase("bool")]
        [TestCase("int")]
        [TestCase("unsigned int")]
        [TestCase("dword")]
        [TestCase("uint")]
        [TestCase("half")]
        [TestCase("float")]
        [TestCase("double")]
        [TestCase("vector")]
        [TestCase("bool1")]
        [TestCase("bool2")]
        [TestCase("bool3")]
        [TestCase("bool4")]
        [TestCase("half1")]
        [TestCase("half2")]
        [TestCase("half3")]
        [TestCase("half4")]
        [TestCase("int1")]
        [TestCase("int2")]
        [TestCase("int3")]
        [TestCase("int4")]
        [TestCase("float1")]
        [TestCase("float2")]
        [TestCase("float3")]
        [TestCase("float4")]
        [TestCase("double1")]
        [TestCase("double2")]
        [TestCase("double3")]
        [TestCase("double4")]
        [TestCase("matrix")]
        [TestCase("bool1x1")]
        [TestCase("bool1x2")]
        [TestCase("bool1x3")]
        [TestCase("bool1x4")]
        [TestCase("bool2x1")]
        [TestCase("bool2x2")]
        [TestCase("bool2x3")]
        [TestCase("bool2x4")]
        [TestCase("bool3x1")]
        [TestCase("bool3x2")]
        [TestCase("bool3x3")]
        [TestCase("bool3x4")]
        [TestCase("bool4x1")]
        [TestCase("bool4x2")]
        [TestCase("bool4x3")]
        [TestCase("bool4x4")]
        [TestCase("double1x1")]
        [TestCase("double1x2")]
        [TestCase("double1x3")]
        [TestCase("double1x4")]
        [TestCase("double2x1")]
        [TestCase("double2x2")]
        [TestCase("double2x3")]
        [TestCase("double2x4")]
        [TestCase("double3x1")]
        [TestCase("double3x2")]
        [TestCase("double3x3")]
        [TestCase("double3x4")]
        [TestCase("double4x1")]
        [TestCase("double4x2")]
        [TestCase("double4x3")]
        [TestCase("double4x4")]
        [TestCase("half1x1")]
        [TestCase("half1x2")]
        [TestCase("half1x3")]
        [TestCase("half1x4")]
        [TestCase("half2x1")]
        [TestCase("half2x2")]
        [TestCase("half2x3")]
        [TestCase("half2x4")]
        [TestCase("half3x1")]
        [TestCase("half3x2")]
        [TestCase("half3x3")]
        [TestCase("half3x4")]
        [TestCase("half4x1")]
        [TestCase("half4x2")]
        [TestCase("half4x3")]
        [TestCase("half4x4")]
        [TestCase("float1x1")]
        [TestCase("float1x2")]
        [TestCase("float1x3")]
        [TestCase("float1x4")]
        [TestCase("float2x1")]
        [TestCase("float2x2")]
        [TestCase("float2x3")]
        [TestCase("float2x4")]
        [TestCase("float3x1")]
        [TestCase("float3x2")]
        [TestCase("float3x3")]
        [TestCase("float3x4")]
        [TestCase("float4x1")]
        [TestCase("float4x2")]
        [TestCase("float4x3")]
        [TestCase("float4x4")]
        [TestCase("int1x1")]
        [TestCase("int1x2")]
        [TestCase("int1x3")]
        [TestCase("int1x4")]
        [TestCase("int2x1")]
        [TestCase("int2x2")]
        [TestCase("int2x3")]
        [TestCase("int2x4")]
        [TestCase("int3x1")]
        [TestCase("int3x2")]
        [TestCase("int3x3")]
        [TestCase("int3x4")]
        [TestCase("int4x1")]
        [TestCase("int4x2")]
        [TestCase("int4x3")]
        [TestCase("int4x4")]
        [TestCase("uint1x1")]
        [TestCase("uint1x2")]
        [TestCase("uint1x3")]
        [TestCase("uint1x4")]
        [TestCase("uint2x1")]
        [TestCase("uint2x2")]
        [TestCase("uint2x3")]
        [TestCase("uint2x4")]
        [TestCase("uint3x1")]
        [TestCase("uint3x2")]
        [TestCase("uint3x3")]
        [TestCase("uint3x4")]
        [TestCase("uint4x1")]
        [TestCase("uint4x2")]
        [TestCase("uint4x3")]
        [TestCase("uint4x4")]
        [TestCase("AppendStructuredBuffer<double2>")]
        [TestCase("Buffer<int>")]
        [TestCase("ByteAddressBuffer")]
        [TestCase("ConsumeStructuredBuffer<double2>")]
        [TestCase("InputPatch<MyStruct, 2>")]
        [TestCase("LineStream<int>")]
        [TestCase("OutputPatch<MyStruct, 2>")]
        [TestCase("PointStream<int>")]
        [TestCase("RWBuffer<float3>")]
        [TestCase("RWByteAddressBuffer")]
        [TestCase("RWStructuredBuffer<double2>")]
        [TestCase("sampler")]
        [TestCase("SamplerState")]
        [TestCase("SamplerComparisonState")]
        [TestCase("StructuredBuffer<double2>")]
        [TestCase("Texture1D")]
        [TestCase("Texture1D<float4>")]
        [TestCase("Texture1DArray")]
        [TestCase("Texture1DArray<float4>")]
        [TestCase("Texture2D")]
        [TestCase("Texture2D<float4>")]
        [TestCase("Texture2DArray")]
        [TestCase("Texture2DArray<float4>")]
        [TestCase("Texture2DMS<float4, 2>")]
        [TestCase("Texture2DMS<float4, 2+1*3/4>")]
        [TestCase("Texture2DMSArray<float4, 2>")]
        [TestCase("Texture3D")]
        [TestCase("Texture3D<float4>")]
        [TestCase("TextureCube")]
        [TestCase("TextureCube<float4>")]
        [TestCase("TextureCubeArray")]
        [TestCase("TextureCubeArray<float4>")]
        [TestCase("TriangleStream<int>")]
        public void TestGlobalDeclarationWithPredefinedType(string typeText)
        {
            var text = typeText + " c;";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.AreEqual(1, file.Declarations.Count);
            Assert.AreEqual(text, file.ToString());
            Assert.AreEqual(0, file.GetDiagnostics().Count());

            Assert.AreEqual(SyntaxKind.VariableDeclarationStatement, file.Declarations[0].Kind);
            var fs = (VariableDeclarationStatementSyntax)file.Declarations[0];
            Assert.NotNull(fs.Declaration.Type);
            Assert.AreEqual(typeText, fs.Declaration.Type.ToString());
            Assert.AreEqual(1, fs.Declaration.Variables.Count);
            Assert.NotNull(fs.Declaration.Variables[0].Identifier);
            Assert.AreEqual("c", fs.Declaration.Variables[0].Identifier.ToString());
            Assert.Null(fs.Declaration.Variables[0].Initializer);
            Assert.NotNull(fs.SemicolonToken);
            Assert.False(fs.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestGlobalDeclarationWithUnnamedStructType()
        {
            var typeText = "struct { int a; }";
            var text = typeText + " c;";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.AreEqual(1, file.Declarations.Count);
            Assert.AreEqual(text, file.ToString());
            Assert.AreEqual(0, file.GetDiagnostics().Count());

            Assert.AreEqual(SyntaxKind.VariableDeclarationStatement, file.Declarations[0].Kind);
            var fs = (VariableDeclarationStatementSyntax)file.Declarations[0];
            Assert.NotNull(fs.Declaration.Type);
            Assert.AreEqual(typeText, fs.Declaration.Type.ToString());
            Assert.AreEqual(SyntaxKind.StructType, fs.Declaration.Type.Kind);
            Assert.AreEqual(1, fs.Declaration.Variables.Count);
            Assert.NotNull(fs.Declaration.Variables[0].Identifier);
            Assert.AreEqual("c", fs.Declaration.Variables[0].Identifier.ToString());
            Assert.Null(fs.Declaration.Variables[0].Initializer);
            Assert.NotNull(fs.SemicolonToken);
            Assert.False(fs.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestGlobalFunctionDeclaration()
        {
            var text = "void Foo(int a);";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.AreEqual(1, file.Declarations.Count);
            Assert.AreEqual(text, file.ToString());
            Assert.AreEqual(0, file.GetDiagnostics().Count());

            Assert.AreEqual(SyntaxKind.FunctionDeclaration, file.Declarations[0].Kind);
            var fs = (FunctionDeclarationSyntax)file.Declarations[0];
            Assert.NotNull(fs.ReturnType);
            Assert.AreEqual("void", fs.ReturnType.ToString());
            Assert.AreEqual("Foo", fs.Name.ToString());
            Assert.NotNull(fs.ParameterList.OpenParenToken);
            Assert.False(fs.ParameterList.OpenParenToken.IsMissing);
            Assert.AreEqual(1, fs.ParameterList.Parameters.Count);
            var fp = fs.ParameterList.Parameters[0];
            Assert.AreEqual("int", fp.Type.ToString());
            Assert.AreEqual("a", fp.Declarator.Identifier.ToString());
            Assert.NotNull(fs.ParameterList.CloseParenToken);
            Assert.False(fs.ParameterList.CloseParenToken.IsMissing);
            Assert.NotNull(fs.SemicolonToken);
            Assert.False(fs.SemicolonToken.IsMissing);
        }

        [Test]
        public void TestGlobalFunctionDefinition()
        {
            var text = "inline void Foo(int a) { return; }";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.AreEqual(1, file.Declarations.Count);
            Assert.AreEqual(text, file.ToString());
            Assert.AreEqual(0, file.GetDiagnostics().Count());

            Assert.AreEqual(SyntaxKind.FunctionDefinition, file.Declarations[0].Kind);
            var fs = (FunctionDefinitionSyntax)file.Declarations[0];
            Assert.AreEqual(1, fs.Modifiers.Count);
            Assert.AreEqual(SyntaxKind.InlineKeyword, fs.Modifiers[0].Kind);
            Assert.NotNull(fs.ReturnType);
            Assert.AreEqual("void", fs.ReturnType.ToString());
            Assert.AreEqual("Foo", fs.Name.ToString());
            Assert.NotNull(fs.ParameterList.OpenParenToken);
            Assert.False(fs.ParameterList.OpenParenToken.IsMissing);
            Assert.AreEqual(1, fs.ParameterList.Parameters.Count);
            var fp = fs.ParameterList.Parameters[0];
            Assert.AreEqual("int", fp.Type.ToString());
            Assert.AreEqual("a", fp.Declarator.Identifier.ToString());
            Assert.NotNull(fs.ParameterList.CloseParenToken);
            Assert.False(fs.ParameterList.CloseParenToken.IsMissing);
            Assert.NotNull(fs.Body);
            Assert.NotNull(fs.Body.OpenBraceToken);
            Assert.AreEqual(1, fs.Body.Statements.Count);
            Assert.NotNull(fs.Body.CloseBraceToken);
            Assert.Null(fs.SemicolonToken);
        }

        [Test]
        public void TestSamplerStateInitializer()
        {
            var text = "SamplerState MySamplerState { MagFilter = linear; MipFilter = anistropic; };";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.AreEqual(1, file.Declarations.Count);
            Assert.AreEqual(text, file.ToString());
            Assert.AreEqual(0, file.GetDiagnostics().Count());

            Assert.AreEqual(SyntaxKind.VariableDeclarationStatement, file.Declarations[0].Kind);
            var fs = (VariableDeclarationStatementSyntax)file.Declarations[0];
            Assert.NotNull(fs.Declaration.Type);
            Assert.AreEqual("SamplerState", fs.Declaration.Type.ToString());
            Assert.AreEqual(1, fs.Declaration.Variables.Count);
            Assert.AreEqual("MySamplerState", fs.Declaration.Variables[0].Identifier.Text);
            Assert.AreEqual(SyntaxKind.StateInitializer, fs.Declaration.Variables[0].Initializer.Kind);
            var init = (StateInitializerSyntax) fs.Declaration.Variables[0].Initializer;
            Assert.AreEqual(2, init.Properties.Count);
            Assert.AreEqual("MagFilter", init.Properties[0].Name.Text);
            Assert.AreEqual(SyntaxKind.IdentifierName, init.Properties[0].Value.Kind);
            Assert.AreEqual("linear", init.Properties[0].Value.ToString());
            Assert.AreEqual("MipFilter", init.Properties[1].Name.Text);
            Assert.AreEqual(SyntaxKind.IdentifierName, init.Properties[1].Value.Kind);
            Assert.AreEqual("anistropic", init.Properties[1].Value.ToString());
            Assert.NotNull(fs.SemicolonToken);
        }

        private static CompilationUnitSyntax ParseFile(string text)
        {
            return SyntaxFactory.ParseCompilationUnit(text);
        }
    }
}