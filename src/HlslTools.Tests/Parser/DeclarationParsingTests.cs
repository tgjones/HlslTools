﻿using System.Linq;
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
        [TestCase("min16float")]
        [TestCase("min10float")]
        [TestCase("min16int")]
        [TestCase("min12int")]
        [TestCase("min16uint")]
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
        [TestCase("min16float1")]
        [TestCase("min16float2")]
        [TestCase("min16float3")]
        [TestCase("min16float4")]
        [TestCase("min10float1")]
        [TestCase("min10float2")]
        [TestCase("min10float3")]
        [TestCase("min10float4")]
        [TestCase("min16int1")]
        [TestCase("min16int2")]
        [TestCase("min16int3")]
        [TestCase("min16int4")]
        [TestCase("min12int1")]
        [TestCase("min12int2")]
        [TestCase("min12int3")]
        [TestCase("min12int4")]
        [TestCase("min16uint1")]
        [TestCase("min16uint2")]
        [TestCase("min16uint3")]
        [TestCase("min16uint4")]
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
        [TestCase("min16float1x1")]
        [TestCase("min16float1x2")]
        [TestCase("min16float1x3")]
        [TestCase("min16float1x4")]
        [TestCase("min16float2x1")]
        [TestCase("min16float2x2")]
        [TestCase("min16float2x3")]
        [TestCase("min16float2x4")]
        [TestCase("min16float3x1")]
        [TestCase("min16float3x2")]
        [TestCase("min16float3x3")]
        [TestCase("min16float3x4")]
        [TestCase("min16float4x1")]
        [TestCase("min16float4x2")]
        [TestCase("min16float4x3")]
        [TestCase("min16float4x4")]
        [TestCase("min10float1x1")]
        [TestCase("min10float1x2")]
        [TestCase("min10float1x3")]
        [TestCase("min10float1x4")]
        [TestCase("min10float2x1")]
        [TestCase("min10float2x2")]
        [TestCase("min10float2x3")]
        [TestCase("min10float2x4")]
        [TestCase("min10float3x1")]
        [TestCase("min10float3x2")]
        [TestCase("min10float3x3")]
        [TestCase("min10float3x4")]
        [TestCase("min10float4x1")]
        [TestCase("min10float4x2")]
        [TestCase("min10float4x3")]
        [TestCase("min10float4x4")]
        [TestCase("min16int1x1")]
        [TestCase("min16int1x2")]
        [TestCase("min16int1x3")]
        [TestCase("min16int1x4")]
        [TestCase("min16int2x1")]
        [TestCase("min16int2x2")]
        [TestCase("min16int2x3")]
        [TestCase("min16int2x4")]
        [TestCase("min16int3x1")]
        [TestCase("min16int3x2")]
        [TestCase("min16int3x3")]
        [TestCase("min16int3x4")]
        [TestCase("min16int4x1")]
        [TestCase("min16int4x2")]
        [TestCase("min16int4x3")]
        [TestCase("min16int4x4")]
        [TestCase("min12int1x1")]
        [TestCase("min12int1x2")]
        [TestCase("min12int1x3")]
        [TestCase("min12int1x4")]
        [TestCase("min12int2x1")]
        [TestCase("min12int2x2")]
        [TestCase("min12int2x3")]
        [TestCase("min12int2x4")]
        [TestCase("min12int3x1")]
        [TestCase("min12int3x2")]
        [TestCase("min12int3x3")]
        [TestCase("min12int3x4")]
        [TestCase("min12int4x1")]
        [TestCase("min12int4x2")]
        [TestCase("min12int4x3")]
        [TestCase("min12int4x4")]
        [TestCase("min16uint1x1")]
        [TestCase("min16uint1x2")]
        [TestCase("min16uint1x3")]
        [TestCase("min16uint1x4")]
        [TestCase("min16uint2x1")]
        [TestCase("min16uint2x2")]
        [TestCase("min16uint2x3")]
        [TestCase("min16uint2x4")]
        [TestCase("min16uint3x1")]
        [TestCase("min16uint3x2")]
        [TestCase("min16uint3x3")]
        [TestCase("min16uint3x4")]
        [TestCase("min16uint4x1")]
        [TestCase("min16uint4x2")]
        [TestCase("min16uint4x3")]
        [TestCase("min16uint4x4")]
        [TestCase("AppendStructuredBuffer<double2>")]
        [TestCase("AppendStructuredBuffer<float4x4>")]
        [TestCase("AppendStructuredBuffer<int4x3>")]
        [TestCase("Buffer<int>")]
        [TestCase("ByteAddressBuffer")]
        [TestCase("ConsumeStructuredBuffer<double2>")]
        [TestCase("ConsumeStructuredBuffer<float4x4>")]
        [TestCase("ConsumeStructuredBuffer<int4x3>")]
        [TestCase("InputPatch<MyStruct, 2>")]
        [TestCase("LineStream<int>")]
        [TestCase("OutputPatch<MyStruct, 2>")]
        [TestCase("PointStream<int>")]
        [TestCase("RWBuffer<float3>")]
        [TestCase("RWByteAddressBuffer")]
        [TestCase("RWStructuredBuffer<double2>")]
        [TestCase("RWStructuredBuffer<float4x4>")]
        [TestCase("RWStructuredBuffer<int4x3>")]
        [TestCase("sampler")]
        [TestCase("SamplerState")]
        [TestCase("SamplerComparisonState")]
        [TestCase("StructuredBuffer<double2>")]
        [TestCase("StructuredBuffer<float4x4>")]
        [TestCase("StructuredBuffer<int4x3>")]
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
        public void TestExportFunctionDefinition()
        {
            var text = "export void Foo(int a) { return; }";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.AreEqual(1, file.Declarations.Count);
            Assert.AreEqual(text, file.ToString());
            Assert.AreEqual(0, file.GetDiagnostics().Count());

            Assert.AreEqual(SyntaxKind.FunctionDefinition, file.Declarations[0].Kind);
            var fs = (FunctionDefinitionSyntax)file.Declarations[0];
            Assert.AreEqual(1, fs.Modifiers.Count);
            Assert.AreEqual(SyntaxKind.ExportKeyword, fs.Modifiers[0].Kind);
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
            var text = "SamplerState MySamplerState { MinFilter = <point>; MagFilter = linear; MipFilter = anistropic; AlphaBlendEnable = 16 > 8; };";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.AreEqual(1, file.Declarations.Count);
            Assert.AreEqual(text, file.ToString());
            Assert.That(file.GetDiagnostics(), Is.Empty);

            Assert.AreEqual(SyntaxKind.VariableDeclarationStatement, file.Declarations[0].Kind);
            var fs = (VariableDeclarationStatementSyntax)file.Declarations[0];
            Assert.NotNull(fs.Declaration.Type);
            Assert.AreEqual("SamplerState", fs.Declaration.Type.ToString());
            Assert.AreEqual(1, fs.Declaration.Variables.Count);
            Assert.AreEqual("MySamplerState", fs.Declaration.Variables[0].Identifier.Text);
            Assert.AreEqual(SyntaxKind.StateInitializer, fs.Declaration.Variables[0].Initializer.Kind);
            var init = (StateInitializerSyntax) fs.Declaration.Variables[0].Initializer;

            Assert.AreEqual(4, init.Properties.Count);

            Assert.AreEqual("MinFilter", init.Properties[0].Name.Text);
            Assert.AreEqual(SyntaxKind.IdentifierName, init.Properties[0].Value.Kind);
            Assert.AreEqual("point", init.Properties[0].Value.ToString());

            Assert.AreEqual("MagFilter", init.Properties[1].Name.Text);
            Assert.AreEqual(SyntaxKind.IdentifierName, init.Properties[1].Value.Kind);
            Assert.AreEqual("linear", init.Properties[1].Value.ToString());

            Assert.AreEqual("MipFilter", init.Properties[2].Name.Text);
            Assert.AreEqual(SyntaxKind.IdentifierName, init.Properties[2].Value.Kind);
            Assert.AreEqual("anistropic", init.Properties[2].Value.ToString());

            Assert.AreEqual("AlphaBlendEnable", init.Properties[3].Name.Text);
            Assert.AreEqual(SyntaxKind.GreaterThanExpression, init.Properties[3].Value.Kind);
            Assert.AreEqual(SyntaxKind.NumericLiteralExpression, ((BinaryExpressionSyntax) init.Properties[3].Value).Left.Kind);
            Assert.AreEqual("16", ((BinaryExpressionSyntax)init.Properties[3].Value).Left.ToString());
            Assert.AreEqual(SyntaxKind.GreaterThanToken, ((BinaryExpressionSyntax)init.Properties[3].Value).OperatorToken.Kind);
            Assert.AreEqual(SyntaxKind.NumericLiteralExpression, ((BinaryExpressionSyntax)init.Properties[3].Value).Right.Kind);
            Assert.AreEqual("8", ((BinaryExpressionSyntax)init.Properties[3].Value).Right.ToString());

            Assert.NotNull(fs.SemicolonToken);
        }

        [Test]
        public void TestTechnique()
        {
            var text = "technique Technique { pass Pass1 { } };";
            var file = ParseFile(text);

            Assert.NotNull(file);
            Assert.AreEqual(1, file.Declarations.Count);
            Assert.AreEqual(text, file.ToString());
            Assert.That(file.GetDiagnostics().Count(), Is.EqualTo(1));

            Assert.AreEqual(SyntaxKind.TechniqueDeclaration, file.Declarations[0].Kind);
            var fs = (TechniqueSyntax)file.Declarations[0];
            Assert.IsFalse(fs.TechniqueKeyword.IsMissing);
            Assert.Null(fs.Name);
            Assert.AreEqual(1, fs.Passes.Count);
            Assert.AreEqual("Pass1", fs.Passes[0].Name.Text);
            Assert.AreEqual(0, fs.Passes[0].Statements.Count);

            Assert.NotNull(fs.SemicolonToken);
        }

        private static CompilationUnitSyntax ParseFile(string text)
        {
            return SyntaxFactory.ParseCompilationUnit(text);
        }
    }
}