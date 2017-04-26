using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Binding
{
    public class FieldExpressionTests : BindingTestsBase
    {
        [Fact]
        public void FieldsVisibleToInlineInstanceFunction()
        {
            var code = @"
struct PassInterface
{
    float3 BaseColor;

    float3 GetColor()
    {
        return BaseColor;
    }
};";
            AssertNoDiagnostics(code);
        }

        [Fact]
        public void FieldsVisibleToInlineStaticFunction()
        {
            var code = @"
struct PassInterface
{
    float3 BaseColor;

    static PassInterface Create(float3 color = 0)
    {
        PassInterface result;
        result.BaseColor = color;
        return result;
    }
};";
            AssertNoDiagnostics(code);
        }
    }
}
