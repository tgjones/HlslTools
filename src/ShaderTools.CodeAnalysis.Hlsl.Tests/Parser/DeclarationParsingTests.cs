using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Parser
{
    public class DeclarationParsingTests
    {
        [Theory]
        [InlineData("bool")]
        [InlineData("int")]
        [InlineData("unsigned int")]
        [InlineData("dword")]
        [InlineData("uint")]
        [InlineData("half")]
        [InlineData("float")]
        [InlineData("double")]
        [InlineData("min16float")]
        [InlineData("min10float")]
        [InlineData("min16int")]
        [InlineData("min12int")]
        [InlineData("min16uint")]
        [InlineData("vector")]
        [InlineData("bool1")]
        [InlineData("bool2")]
        [InlineData("bool3")]
        [InlineData("bool4")]
        [InlineData("half1")]
        [InlineData("half2")]
        [InlineData("half3")]
        [InlineData("half4")]
        [InlineData("int1")]
        [InlineData("int2")]
        [InlineData("int3")]
        [InlineData("int4")]
        [InlineData("float1")]
        [InlineData("float2")]
        [InlineData("float3")]
        [InlineData("float4")]
        [InlineData("double1")]
        [InlineData("double2")]
        [InlineData("double3")]
        [InlineData("double4")]
        [InlineData("min16float1")]
        [InlineData("min16float2")]
        [InlineData("min16float3")]
        [InlineData("min16float4")]
        [InlineData("min10float1")]
        [InlineData("min10float2")]
        [InlineData("min10float3")]
        [InlineData("min10float4")]
        [InlineData("min16int1")]
        [InlineData("min16int2")]
        [InlineData("min16int3")]
        [InlineData("min16int4")]
        [InlineData("min12int1")]
        [InlineData("min12int2")]
        [InlineData("min12int3")]
        [InlineData("min12int4")]
        [InlineData("min16uint1")]
        [InlineData("min16uint2")]
        [InlineData("min16uint3")]
        [InlineData("min16uint4")]
        [InlineData("matrix")]
        [InlineData("bool1x1")]
        [InlineData("bool1x2")]
        [InlineData("bool1x3")]
        [InlineData("bool1x4")]
        [InlineData("bool2x1")]
        [InlineData("bool2x2")]
        [InlineData("bool2x3")]
        [InlineData("bool2x4")]
        [InlineData("bool3x1")]
        [InlineData("bool3x2")]
        [InlineData("bool3x3")]
        [InlineData("bool3x4")]
        [InlineData("bool4x1")]
        [InlineData("bool4x2")]
        [InlineData("bool4x3")]
        [InlineData("bool4x4")]
        [InlineData("double1x1")]
        [InlineData("double1x2")]
        [InlineData("double1x3")]
        [InlineData("double1x4")]
        [InlineData("double2x1")]
        [InlineData("double2x2")]
        [InlineData("double2x3")]
        [InlineData("double2x4")]
        [InlineData("double3x1")]
        [InlineData("double3x2")]
        [InlineData("double3x3")]
        [InlineData("double3x4")]
        [InlineData("double4x1")]
        [InlineData("double4x2")]
        [InlineData("double4x3")]
        [InlineData("double4x4")]
        [InlineData("half1x1")]
        [InlineData("half1x2")]
        [InlineData("half1x3")]
        [InlineData("half1x4")]
        [InlineData("half2x1")]
        [InlineData("half2x2")]
        [InlineData("half2x3")]
        [InlineData("half2x4")]
        [InlineData("half3x1")]
        [InlineData("half3x2")]
        [InlineData("half3x3")]
        [InlineData("half3x4")]
        [InlineData("half4x1")]
        [InlineData("half4x2")]
        [InlineData("half4x3")]
        [InlineData("half4x4")]
        [InlineData("float1x1")]
        [InlineData("float1x2")]
        [InlineData("float1x3")]
        [InlineData("float1x4")]
        [InlineData("float2x1")]
        [InlineData("float2x2")]
        [InlineData("float2x3")]
        [InlineData("float2x4")]
        [InlineData("float3x1")]
        [InlineData("float3x2")]
        [InlineData("float3x3")]
        [InlineData("float3x4")]
        [InlineData("float4x1")]
        [InlineData("float4x2")]
        [InlineData("float4x3")]
        [InlineData("float4x4")]
        [InlineData("int1x1")]
        [InlineData("int1x2")]
        [InlineData("int1x3")]
        [InlineData("int1x4")]
        [InlineData("int2x1")]
        [InlineData("int2x2")]
        [InlineData("int2x3")]
        [InlineData("int2x4")]
        [InlineData("int3x1")]
        [InlineData("int3x2")]
        [InlineData("int3x3")]
        [InlineData("int3x4")]
        [InlineData("int4x1")]
        [InlineData("int4x2")]
        [InlineData("int4x3")]
        [InlineData("int4x4")]
        [InlineData("uint1x1")]
        [InlineData("uint1x2")]
        [InlineData("uint1x3")]
        [InlineData("uint1x4")]
        [InlineData("uint2x1")]
        [InlineData("uint2x2")]
        [InlineData("uint2x3")]
        [InlineData("uint2x4")]
        [InlineData("uint3x1")]
        [InlineData("uint3x2")]
        [InlineData("uint3x3")]
        [InlineData("uint3x4")]
        [InlineData("uint4x1")]
        [InlineData("uint4x2")]
        [InlineData("uint4x3")]
        [InlineData("uint4x4")]
        [InlineData("min16float1x1")]
        [InlineData("min16float1x2")]
        [InlineData("min16float1x3")]
        [InlineData("min16float1x4")]
        [InlineData("min16float2x1")]
        [InlineData("min16float2x2")]
        [InlineData("min16float2x3")]
        [InlineData("min16float2x4")]
        [InlineData("min16float3x1")]
        [InlineData("min16float3x2")]
        [InlineData("min16float3x3")]
        [InlineData("min16float3x4")]
        [InlineData("min16float4x1")]
        [InlineData("min16float4x2")]
        [InlineData("min16float4x3")]
        [InlineData("min16float4x4")]
        [InlineData("min10float1x1")]
        [InlineData("min10float1x2")]
        [InlineData("min10float1x3")]
        [InlineData("min10float1x4")]
        [InlineData("min10float2x1")]
        [InlineData("min10float2x2")]
        [InlineData("min10float2x3")]
        [InlineData("min10float2x4")]
        [InlineData("min10float3x1")]
        [InlineData("min10float3x2")]
        [InlineData("min10float3x3")]
        [InlineData("min10float3x4")]
        [InlineData("min10float4x1")]
        [InlineData("min10float4x2")]
        [InlineData("min10float4x3")]
        [InlineData("min10float4x4")]
        [InlineData("min16int1x1")]
        [InlineData("min16int1x2")]
        [InlineData("min16int1x3")]
        [InlineData("min16int1x4")]
        [InlineData("min16int2x1")]
        [InlineData("min16int2x2")]
        [InlineData("min16int2x3")]
        [InlineData("min16int2x4")]
        [InlineData("min16int3x1")]
        [InlineData("min16int3x2")]
        [InlineData("min16int3x3")]
        [InlineData("min16int3x4")]
        [InlineData("min16int4x1")]
        [InlineData("min16int4x2")]
        [InlineData("min16int4x3")]
        [InlineData("min16int4x4")]
        [InlineData("min12int1x1")]
        [InlineData("min12int1x2")]
        [InlineData("min12int1x3")]
        [InlineData("min12int1x4")]
        [InlineData("min12int2x1")]
        [InlineData("min12int2x2")]
        [InlineData("min12int2x3")]
        [InlineData("min12int2x4")]
        [InlineData("min12int3x1")]
        [InlineData("min12int3x2")]
        [InlineData("min12int3x3")]
        [InlineData("min12int3x4")]
        [InlineData("min12int4x1")]
        [InlineData("min12int4x2")]
        [InlineData("min12int4x3")]
        [InlineData("min12int4x4")]
        [InlineData("min16uint1x1")]
        [InlineData("min16uint1x2")]
        [InlineData("min16uint1x3")]
        [InlineData("min16uint1x4")]
        [InlineData("min16uint2x1")]
        [InlineData("min16uint2x2")]
        [InlineData("min16uint2x3")]
        [InlineData("min16uint2x4")]
        [InlineData("min16uint3x1")]
        [InlineData("min16uint3x2")]
        [InlineData("min16uint3x3")]
        [InlineData("min16uint3x4")]
        [InlineData("min16uint4x1")]
        [InlineData("min16uint4x2")]
        [InlineData("min16uint4x3")]
        [InlineData("min16uint4x4")]
        [InlineData("AppendStructuredBuffer<double2>")]
        [InlineData("AppendStructuredBuffer<float4x4>")]
        [InlineData("AppendStructuredBuffer<int4x3>")]
        [InlineData("Buffer<int>")]
        [InlineData("ByteAddressBuffer")]
        [InlineData("ConsumeStructuredBuffer<double2>")]
        [InlineData("ConsumeStructuredBuffer<float4x4>")]
        [InlineData("ConsumeStructuredBuffer<int4x3>")]
        [InlineData("InputPatch<MyStruct, 2>")]
        [InlineData("LineStream<int>")]
        [InlineData("OutputPatch<MyStruct, 2>")]
        [InlineData("PointStream<int>")]
        [InlineData("RWBuffer<float3>")]
        [InlineData("RWByteAddressBuffer")]
        [InlineData("RWStructuredBuffer<double2>")]
        [InlineData("RWStructuredBuffer<float4x4>")]
        [InlineData("RWStructuredBuffer<int4x3>")]
        [InlineData("sampler")]
        [InlineData("SamplerState")]
        [InlineData("SamplerComparisonState")]
        [InlineData("StructuredBuffer<double2>")]
        [InlineData("StructuredBuffer<float4x4>")]
        [InlineData("StructuredBuffer<int4x3>")]
        [InlineData("Texture1D")]
        [InlineData("Texture1D<float4>")]
        [InlineData("Texture1DArray")]
        [InlineData("Texture1DArray<float4>")]
        [InlineData("Texture2D")]
        [InlineData("Texture2D<float4>")]
        [InlineData("Texture2DArray")]
        [InlineData("Texture2DArray<float4>")]
        [InlineData("Texture2DMS<float4, 2>")]
        [InlineData("Texture2DMS<float4, 2+1*3/4>")]
        [InlineData("Texture2DMSArray<float4, 2>")]
        [InlineData("Texture3D")]
        [InlineData("Texture3D<float4>")]
        [InlineData("TextureCube")]
        [InlineData("TextureCube<float4>")]
        [InlineData("TextureCubeArray")]
        [InlineData("TextureCubeArray<float4>")]
        [InlineData("TriangleStream<int>")]
        public void TestGlobalDeclarationWithPredefinedType(string typeText)
        {
            var text = typeText + " c;";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Equal(1, file.Declarations.Count);
            Assert.Equal(text, file.ToString());
            Assert.Equal(0, file.GetDiagnostics().Count());

            Assert.Equal(SyntaxKind.VariableDeclarationStatement, file.Declarations[0].Kind);
            var fs = (VariableDeclarationStatementSyntax)file.Declarations[0];
            Assert.NotNull(fs.Declaration.Type);
            Assert.Equal(typeText, fs.Declaration.Type.ToString());
            Assert.Equal(1, fs.Declaration.Variables.Count);
            Assert.NotNull(fs.Declaration.Variables[0].Identifier);
            Assert.Equal("c", fs.Declaration.Variables[0].Identifier.ToString());
            Assert.Null(fs.Declaration.Variables[0].Initializer);
            Assert.NotNull(fs.SemicolonToken);
            Assert.False(fs.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestGlobalDeclarationWithUnormFloatType()
        {
            var typeText = "Texture2D<unorm float4>";
            var text = typeText + " c;";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Equal(1, file.Declarations.Count);
            Assert.Equal(text, file.ToString());
            Assert.Equal(0, file.GetDiagnostics().Count());

            Assert.Equal(SyntaxKind.VariableDeclarationStatement, file.Declarations[0].Kind);
            var fs = (VariableDeclarationStatementSyntax)file.Declarations[0];
            Assert.NotNull(fs.Declaration.Type);
            Assert.Equal(typeText, fs.Declaration.Type.ToString());
            Assert.Equal(SyntaxKind.PredefinedObjectType, fs.Declaration.Type.Kind);
            Assert.Equal(1, fs.Declaration.Variables.Count);
            Assert.NotNull(fs.Declaration.Variables[0].Identifier);
            Assert.Equal("c", fs.Declaration.Variables[0].Identifier.ToString());
            Assert.Null(fs.Declaration.Variables[0].Initializer);
            Assert.NotNull(fs.SemicolonToken);
            Assert.False(fs.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestGlobalDeclarationWithUnnamedStructType()
        {
            var typeText = "struct { int a; }";
            var text = typeText + " c;";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Equal(1, file.Declarations.Count);
            Assert.Equal(text, file.ToString());
            Assert.Equal(0, file.GetDiagnostics().Count());

            Assert.Equal(SyntaxKind.VariableDeclarationStatement, file.Declarations[0].Kind);
            var fs = (VariableDeclarationStatementSyntax)file.Declarations[0];
            Assert.NotNull(fs.Declaration.Type);
            Assert.Equal(typeText, fs.Declaration.Type.ToString());
            Assert.Equal(SyntaxKind.StructType, fs.Declaration.Type.Kind);
            Assert.Equal(1, fs.Declaration.Variables.Count);
            Assert.NotNull(fs.Declaration.Variables[0].Identifier);
            Assert.Equal("c", fs.Declaration.Variables[0].Identifier.ToString());
            Assert.Null(fs.Declaration.Variables[0].Initializer);
            Assert.NotNull(fs.SemicolonToken);
            Assert.False(fs.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestGlobalDeclarationWithTypedefStructType()
        {
            var typeText = "struct { int a; }";
            var typedefText = "typedef " + typeText;
            var text = typedefText + " c;";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Equal(1, file.Declarations.Count);
            Assert.Equal(text, file.ToString());
            Assert.Equal(0, file.GetDiagnostics().Count());

            Assert.Equal(SyntaxKind.TypedefStatement, file.Declarations[0].Kind);
            var ts = (TypedefStatementSyntax)file.Declarations[0];
            Assert.NotNull(ts.Type);
            Assert.Equal(typeText, ts.Type.ToString());
            Assert.Equal(SyntaxKind.StructType, ts.Type.Kind);
            Assert.Equal(1, ts.Declarators.Count);
            var ds = ts.Declarators[0];
            Assert.NotNull(ds.Identifier);
            Assert.Equal("c", ds.Identifier.ToString());
            Assert.NotNull(ts.SemicolonToken);
            Assert.False(ts.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestGlobalFunctionDeclaration()
        {
            var text = "void Foo(int a);";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Equal(1, file.Declarations.Count);
            Assert.Equal(text, file.ToString());
            Assert.Equal(0, file.GetDiagnostics().Count());

            Assert.Equal(SyntaxKind.FunctionDeclaration, file.Declarations[0].Kind);
            var fs = (FunctionDeclarationSyntax)file.Declarations[0];
            Assert.NotNull(fs.ReturnType);
            Assert.Equal("void", fs.ReturnType.ToString());
            Assert.Equal("Foo", fs.Name.ToString());
            Assert.NotNull(fs.ParameterList.OpenParenToken);
            Assert.False(fs.ParameterList.OpenParenToken.IsMissing);
            Assert.Equal(1, fs.ParameterList.Parameters.Count);
            var fp = fs.ParameterList.Parameters[0];
            Assert.Equal("int", fp.Type.ToString());
            Assert.Equal("a", fp.Declarator.Identifier.ToString());
            Assert.NotNull(fs.ParameterList.CloseParenToken);
            Assert.False(fs.ParameterList.CloseParenToken.IsMissing);
            Assert.NotNull(fs.SemicolonToken);
            Assert.False(fs.SemicolonToken.IsMissing);
        }

        [Fact]
        public void TestGlobalFunctionDefinition()
        {
            var text = "inline void Foo(int a) { return; }";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Equal(1, file.Declarations.Count);
            Assert.Equal(text, file.ToString());
            Assert.Equal(0, file.GetDiagnostics().Count());

            Assert.Equal(SyntaxKind.FunctionDefinition, file.Declarations[0].Kind);
            var fs = (FunctionDefinitionSyntax)file.Declarations[0];
            Assert.Equal(1, fs.Modifiers.Count);
            Assert.Equal(SyntaxKind.InlineKeyword, fs.Modifiers[0].Kind);
            Assert.NotNull(fs.ReturnType);
            Assert.Equal("void", fs.ReturnType.ToString());
            Assert.Equal("Foo", fs.Name.ToString());
            Assert.NotNull(fs.ParameterList.OpenParenToken);
            Assert.False(fs.ParameterList.OpenParenToken.IsMissing);
            Assert.Equal(1, fs.ParameterList.Parameters.Count);
            var fp = fs.ParameterList.Parameters[0];
            Assert.Equal("int", fp.Type.ToString());
            Assert.Equal("a", fp.Declarator.Identifier.ToString());
            Assert.NotNull(fs.ParameterList.CloseParenToken);
            Assert.False(fs.ParameterList.CloseParenToken.IsMissing);
            Assert.NotNull(fs.Body);
            Assert.NotNull(fs.Body.OpenBraceToken);
            Assert.Equal(1, fs.Body.Statements.Count);
            Assert.NotNull(fs.Body.CloseBraceToken);
            Assert.Null(fs.SemicolonToken);
        }

        [Fact]
        public void TestExportFunctionDefinition()
        {
            var text = "export void Foo(int a) { return; }";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Equal(1, file.Declarations.Count);
            Assert.Equal(text, file.ToString());
            Assert.Equal(0, file.GetDiagnostics().Count());

            Assert.Equal(SyntaxKind.FunctionDefinition, file.Declarations[0].Kind);
            var fs = (FunctionDefinitionSyntax)file.Declarations[0];
            Assert.Equal(1, fs.Modifiers.Count);
            Assert.Equal(SyntaxKind.ExportKeyword, fs.Modifiers[0].Kind);
            Assert.NotNull(fs.ReturnType);
            Assert.Equal("void", fs.ReturnType.ToString());
            Assert.Equal("Foo", fs.Name.ToString());
            Assert.NotNull(fs.ParameterList.OpenParenToken);
            Assert.False(fs.ParameterList.OpenParenToken.IsMissing);
            Assert.Equal(1, fs.ParameterList.Parameters.Count);
            var fp = fs.ParameterList.Parameters[0];
            Assert.Equal("int", fp.Type.ToString());
            Assert.Equal("a", fp.Declarator.Identifier.ToString());
            Assert.NotNull(fs.ParameterList.CloseParenToken);
            Assert.False(fs.ParameterList.CloseParenToken.IsMissing);
            Assert.NotNull(fs.Body);
            Assert.NotNull(fs.Body.OpenBraceToken);
            Assert.Equal(1, fs.Body.Statements.Count);
            Assert.NotNull(fs.Body.CloseBraceToken);
            Assert.Null(fs.SemicolonToken);
        }

        [Fact]
        public void TestSamplerStateInitializer()
        {
            var text = "SamplerState MySamplerState { MinFilter = <point>; MagFilter = linear; MipFilter = anistropic; AlphaBlendEnable = 16 > 8; };";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Equal(1, file.Declarations.Count);
            Assert.Equal(text, file.ToString());
            Assert.Empty(file.GetDiagnostics());

            Assert.Equal(SyntaxKind.VariableDeclarationStatement, file.Declarations[0].Kind);
            var fs = (VariableDeclarationStatementSyntax)file.Declarations[0];
            Assert.NotNull(fs.Declaration.Type);
            Assert.Equal("SamplerState", fs.Declaration.Type.ToString());
            Assert.Equal(1, fs.Declaration.Variables.Count);
            Assert.Equal("MySamplerState", fs.Declaration.Variables[0].Identifier.Text);
            Assert.Equal(SyntaxKind.StateInitializer, fs.Declaration.Variables[0].Initializer.Kind);
            var init = (StateInitializerSyntax) fs.Declaration.Variables[0].Initializer;

            Assert.Equal(4, init.Properties.Count);

            Assert.Equal("MinFilter", init.Properties[0].Name.Text);
            Assert.Equal(SyntaxKind.IdentifierName, init.Properties[0].Value.Kind);
            Assert.Equal("point", init.Properties[0].Value.ToString());

            Assert.Equal("MagFilter", init.Properties[1].Name.Text);
            Assert.Equal(SyntaxKind.IdentifierName, init.Properties[1].Value.Kind);
            Assert.Equal("linear", init.Properties[1].Value.ToString());

            Assert.Equal("MipFilter", init.Properties[2].Name.Text);
            Assert.Equal(SyntaxKind.IdentifierName, init.Properties[2].Value.Kind);
            Assert.Equal("anistropic", init.Properties[2].Value.ToString());

            Assert.Equal("AlphaBlendEnable", init.Properties[3].Name.Text);
            Assert.Equal(SyntaxKind.GreaterThanExpression, init.Properties[3].Value.Kind);
            Assert.Equal(SyntaxKind.NumericLiteralExpression, ((BinaryExpressionSyntax) init.Properties[3].Value).Left.Kind);
            Assert.Equal("16", ((BinaryExpressionSyntax)init.Properties[3].Value).Left.ToString());
            Assert.Equal(SyntaxKind.GreaterThanToken, ((BinaryExpressionSyntax)init.Properties[3].Value).OperatorToken.Kind);
            Assert.Equal(SyntaxKind.NumericLiteralExpression, ((BinaryExpressionSyntax)init.Properties[3].Value).Right.Kind);
            Assert.Equal("8", ((BinaryExpressionSyntax)init.Properties[3].Value).Right.ToString());

            Assert.NotNull(fs.SemicolonToken);
        }

        [Fact]
        public void TestTechnique()
        {
            var text = "technique Technique { pass Pass1 { } };";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Equal(1, file.Declarations.Count);
            Assert.Equal(text, file.ToString());
            Assert.Equal(1, file.GetDiagnostics().Count());

            Assert.Equal(SyntaxKind.TechniqueDeclaration, file.Declarations[0].Kind);
            var fs = (TechniqueSyntax)file.Declarations[0];
            Assert.False(fs.TechniqueKeyword.IsMissing);
            Assert.Null(fs.Name);
            Assert.Equal(1, fs.Passes.Count);
            Assert.Equal("Pass1", fs.Passes[0].Name.Text);
            Assert.Equal(0, fs.Passes[0].Statements.Count);

            Assert.NotNull(fs.SemicolonToken);
        }

        [Fact]
        public void TestAttributeSpecifierOnFunctionReturn()
        {
            var text = "[[vk::location(0)]] float4 main() { return float4(1, 1, 1, 1); }";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Empty(file.GetDiagnostics());
            Assert.Equal(text, file.ToString());
            Assert.Equal(1, file.Declarations.Count);

            Assert.Equal(SyntaxKind.FunctionDefinition, file.Declarations[0].Kind);
            var fd = (FunctionDefinitionSyntax)file.Declarations[0];
            Assert.Equal(1, fd.Attributes.Count);
        }

        [Fact]
        public void TestAttributeSpecifierOnFunctionParameter()
        {
            var text = "float4 main([[vk::location(0)]] float4 input) { return float4(1, 1, 1, 1); }";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Empty(file.GetDiagnostics());
            Assert.Equal(text, file.ToString());
            Assert.Equal(1, file.Declarations.Count);

            Assert.Equal(SyntaxKind.FunctionDefinition, file.Declarations[0].Kind);
            var fd = (FunctionDefinitionSyntax)file.Declarations[0];
            Assert.Equal(1, fd.ParameterList.Parameters[0].Attributes.Count);
        }

        [Fact]
        public void TestAttributeSpecifierOnStructuredBuffer()
        {
            var text = "[[vk::binding(0, 1), vk::counter_binding(2)]] RWStructuredBuffer<float4> mySBuffer;";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Empty(file.GetDiagnostics());
            Assert.Equal(text, file.ToString());
            Assert.Equal(1, file.Declarations.Count);

            Assert.Equal(SyntaxKind.VariableDeclarationStatement, file.Declarations[0].Kind);
            var fd = (VariableDeclarationStatementSyntax)file.Declarations[0];
            Assert.Equal(1, fd.Attributes.Count);
        }

        [Fact]
        public void TestAttributeSpecifierOnStructField()
        {
            var text = "struct S { [[vk::binding(0)]] float4 Position; }; ";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Empty(file.GetDiagnostics());
            Assert.Equal(text, file.ToString());
            Assert.Equal(1, file.Declarations.Count);

            Assert.Equal(SyntaxKind.TypeDeclarationStatement, file.Declarations[0].Kind);
            var fd = (TypeDeclarationStatementSyntax)file.Declarations[0];
            Assert.Equal(SyntaxKind.StructType, fd.Type.Kind);
            var st = (StructTypeSyntax)fd.Type;
            Assert.Equal(SyntaxKind.VariableDeclarationStatement, st.Members[0].Kind);
            var vd = (VariableDeclarationStatementSyntax)st.Members[0];
            Assert.Equal(1, vd.Attributes.Count);
        }

        [Fact]
        public void TestAttributeSpecifierOnGlobalVariable()
        {
            var text = "struct S { }; [[vk::push_constant]] S PushConstants; ";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.Empty(file.GetDiagnostics());
            Assert.Equal(text, file.ToString());
            Assert.Equal(2, file.Declarations.Count);

            Assert.Equal(SyntaxKind.VariableDeclarationStatement, file.Declarations[1].Kind);
            var fd = (VariableDeclarationStatementSyntax)file.Declarations[1];
            Assert.Equal(1, fd.Attributes.Count);
        }

        private static CompilationUnitSyntax ParseFile(string text)
        {
            return SyntaxFactory.ParseCompilationUnit(new SourceFile(SourceText.From(text)));
        }
    }
}