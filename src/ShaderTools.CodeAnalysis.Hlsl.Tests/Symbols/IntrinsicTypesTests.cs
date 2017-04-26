using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Symbols
{
    public class IntrinsicTypesTests
    {
        [Fact]
        public void VectorTypesHaveCorrectFields()
        {
            var float1Type = IntrinsicTypes.Float1;
            Assert.Equal(9, float1Type.Members.Length);
            Assert.Equal("x", float1Type.Members[0].Name);
            Assert.Equal("xx", float1Type.Members[1].Name);
            Assert.Equal("xxx", float1Type.Members[2].Name);
            Assert.Equal("xxxx", float1Type.Members[3].Name);
            Assert.Equal("r", float1Type.Members[4].Name);
            Assert.Equal("rr", float1Type.Members[5].Name);
            Assert.Equal("rrr", float1Type.Members[6].Name);
            Assert.Equal("rrrr", float1Type.Members[7].Name);

            var float2Type = IntrinsicTypes.Float2;
            Assert.Equal(61, float2Type.Members.Length);
            Assert.Equal("y", float2Type.Members[1].Name);
            Assert.Equal("x", float2Type.Members[0].Name);
            Assert.Equal("xx", float2Type.Members[2].Name);
            Assert.Equal("xy", float2Type.Members[3].Name);
            Assert.Equal("yx", float2Type.Members[4].Name);
            Assert.Equal("yy", float2Type.Members[5].Name);
            Assert.Equal("xxx", float2Type.Members[6].Name);
            Assert.Equal("xxy", float2Type.Members[7].Name);
            Assert.Equal("xyx", float2Type.Members[8].Name);
            Assert.Equal("xyy", float2Type.Members[9].Name);
            Assert.Equal("yxx", float2Type.Members[10].Name);
            Assert.Equal("yxy", float2Type.Members[11].Name);
            Assert.Equal("yyx", float2Type.Members[12].Name);
            Assert.Equal("yyy", float2Type.Members[13].Name);
            Assert.Equal("xxxx", float2Type.Members[14].Name);
            Assert.Equal("xxxy", float2Type.Members[15].Name);
            Assert.Equal("xxyx", float2Type.Members[16].Name);
            Assert.Equal("xxyy", float2Type.Members[17].Name);
            Assert.Equal("xyxx", float2Type.Members[18].Name);
            Assert.Equal("xyxy", float2Type.Members[19].Name);
            Assert.Equal("xyyx", float2Type.Members[20].Name);
            Assert.Equal("xyyy", float2Type.Members[21].Name);
            Assert.Equal("yxxx", float2Type.Members[22].Name);
            Assert.Equal("yxxy", float2Type.Members[23].Name);
            Assert.Equal("yxyx", float2Type.Members[24].Name);
            Assert.Equal("yxyy", float2Type.Members[25].Name);
            Assert.Equal("yyxx", float2Type.Members[26].Name);
            Assert.Equal("yyxy", float2Type.Members[27].Name);
            Assert.Equal("yyyx", float2Type.Members[28].Name);
            Assert.Equal("yyyy", float2Type.Members[29].Name);
            Assert.Equal("r", float2Type.Members[30].Name);
            Assert.Equal("gggg", float2Type.Members[59].Name);

            var float3Type = IntrinsicTypes.Float3;
            Assert.Equal(241, float3Type.Members.Length);

            var float4Type = IntrinsicTypes.Float4;
            Assert.Equal(681, float4Type.Members.Length);
        }

        [Fact]
        public void MatrixTypesHaveCorrectFields()
        {
            var matrix1x1Type = IntrinsicTypes.Float1x1;
            Assert.Equal(9, matrix1x1Type.Members.Length);
            Assert.Equal("_m00", matrix1x1Type.Members[0].Name);
            Assert.Equal("_m00_m00", matrix1x1Type.Members[1].Name);
            Assert.Equal("_m00_m00_m00", matrix1x1Type.Members[2].Name);
            Assert.Equal("_m00_m00_m00_m00", matrix1x1Type.Members[3].Name);
            Assert.Equal("_11", matrix1x1Type.Members[4].Name);
            Assert.Equal("_11_11", matrix1x1Type.Members[5].Name);
            Assert.Equal("_11_11_11", matrix1x1Type.Members[6].Name);
            Assert.Equal("_11_11_11_11", matrix1x1Type.Members[7].Name);

            var matrix1x2Type = IntrinsicTypes.Float1x2;
            Assert.Equal(61, matrix1x2Type.Members.Length);
            Assert.Equal("_m00", matrix1x1Type.Members[0].Name);
            Assert.Equal("_m01", matrix1x2Type.Members[1].Name);
            Assert.Equal("_m00_m00", matrix1x2Type.Members[2].Name);
            Assert.Equal("_m00_m01", matrix1x2Type.Members[3].Name);
            Assert.Equal("_m01_m00", matrix1x2Type.Members[4].Name);
            Assert.Equal("_m01_m01", matrix1x2Type.Members[5].Name);
            Assert.Equal("_m00_m00_m00", matrix1x2Type.Members[6].Name);

            var matrix2x2Type = IntrinsicTypes.Float2x2;
            Assert.Equal(681, matrix2x2Type.Members.Length);

            var matrix4x4Type = IntrinsicTypes.Float4x4;
            Assert.Equal(139809, matrix4x4Type.Members.Length);
        }
    }
}