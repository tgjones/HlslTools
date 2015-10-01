using HlslTools.Symbols;
using NUnit.Framework;

namespace HlslTools.Tests.Symbols
{
    [TestFixture]
    public class IntrinsicTypesTests
    {
        [Test]
        public void VectorTypesHaveCorrectFields()
        {
            var float1Type = IntrinsicTypes.Float1;
            Assert.That(float1Type.Members, Has.Length.EqualTo(8));
            Assert.That(float1Type.Members[0].Name, Is.EqualTo("x"));
            Assert.That(float1Type.Members[1].Name, Is.EqualTo("xx"));
            Assert.That(float1Type.Members[2].Name, Is.EqualTo("xxx"));
            Assert.That(float1Type.Members[3].Name, Is.EqualTo("xxxx"));
            Assert.That(float1Type.Members[4].Name, Is.EqualTo("r"));
            Assert.That(float1Type.Members[5].Name, Is.EqualTo("rr"));
            Assert.That(float1Type.Members[6].Name, Is.EqualTo("rrr"));
            Assert.That(float1Type.Members[7].Name, Is.EqualTo("rrrr"));

            var float2Type = IntrinsicTypes.Float2;
            Assert.That(float2Type.Members, Has.Length.EqualTo(60));
            Assert.That(float2Type.Members[0].Name, Is.EqualTo("x"));
            Assert.That(float2Type.Members[1].Name, Is.EqualTo("y"));
            Assert.That(float2Type.Members[2].Name, Is.EqualTo("xx"));
            Assert.That(float2Type.Members[3].Name, Is.EqualTo("xy"));
            Assert.That(float2Type.Members[4].Name, Is.EqualTo("yx"));
            Assert.That(float2Type.Members[5].Name, Is.EqualTo("yy"));
            Assert.That(float2Type.Members[6].Name, Is.EqualTo("xxx"));
            Assert.That(float2Type.Members[7].Name, Is.EqualTo("xxy"));
            Assert.That(float2Type.Members[8].Name, Is.EqualTo("xyx"));
            Assert.That(float2Type.Members[9].Name, Is.EqualTo("xyy"));
            Assert.That(float2Type.Members[10].Name, Is.EqualTo("yxx"));
            Assert.That(float2Type.Members[11].Name, Is.EqualTo("yxy"));
            Assert.That(float2Type.Members[12].Name, Is.EqualTo("yyx"));
            Assert.That(float2Type.Members[13].Name, Is.EqualTo("yyy"));
            Assert.That(float2Type.Members[14].Name, Is.EqualTo("xxxx"));
            Assert.That(float2Type.Members[15].Name, Is.EqualTo("xxxy"));
            Assert.That(float2Type.Members[16].Name, Is.EqualTo("xxyx"));
            Assert.That(float2Type.Members[17].Name, Is.EqualTo("xxyy"));
            Assert.That(float2Type.Members[18].Name, Is.EqualTo("xyxx"));
            Assert.That(float2Type.Members[19].Name, Is.EqualTo("xyxy"));
            Assert.That(float2Type.Members[20].Name, Is.EqualTo("xyyx"));
            Assert.That(float2Type.Members[21].Name, Is.EqualTo("xyyy"));
            Assert.That(float2Type.Members[22].Name, Is.EqualTo("yxxx"));
            Assert.That(float2Type.Members[23].Name, Is.EqualTo("yxxy"));
            Assert.That(float2Type.Members[24].Name, Is.EqualTo("yxyx"));
            Assert.That(float2Type.Members[25].Name, Is.EqualTo("yxyy"));
            Assert.That(float2Type.Members[26].Name, Is.EqualTo("yyxx"));
            Assert.That(float2Type.Members[27].Name, Is.EqualTo("yyxy"));
            Assert.That(float2Type.Members[28].Name, Is.EqualTo("yyyx"));
            Assert.That(float2Type.Members[29].Name, Is.EqualTo("yyyy"));
            Assert.That(float2Type.Members[30].Name, Is.EqualTo("r"));
            Assert.That(float2Type.Members[59].Name, Is.EqualTo("gggg"));

            var float3Type = IntrinsicTypes.Float3;
            Assert.That(float3Type.Members, Has.Length.EqualTo(240));

            var float4Type = IntrinsicTypes.Float4;
            Assert.That(float4Type.Members, Has.Length.EqualTo(680));
        }
    }
}