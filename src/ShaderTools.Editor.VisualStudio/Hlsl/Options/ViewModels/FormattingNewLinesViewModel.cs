using System;
using System.Windows.Controls;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.Editor.VisualStudio.Core.Options.ViewModels;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Options.ViewModels
{
    internal sealed class FormattingNewLinesViewModel : HlslOptionsPreviewViewModelBase
    {
        private const string TypePreview = @"
//[
struct S1 {
    float4 position;
    float3 normal;
};

struct S2
{
    float4 position;
    float3 normal;
};
//]";

        private const string TechniquePreview = @"
float4 VS() : SV_Position { return float4(1, 0, 0, 1); }
float4 PS() : SV_Target { return float4(1, 0, 0, 1); }
//[
technique T1 {
    pass P {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}

technique T2
{
    pass P {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
//]";

        private const string FunctionPreview = @"
//[
float4 function1() {
    return float4(1, 0, 0, 1);
}

float4 function2()
{
    return float4(1, 0, 0, 1);
}
//]";

        private const string ControlBlockPreview = @"
void main()
{
//[
    for (int i = 0; i < 10; i++) {
        while (true)
        {
        }
    }
//]
}";

        private const string StateBlockPreview = @"
//[
DepthStencilState DepthDisabling {
	DepthEnable = FALSE;
	DepthWriteMask = ZERO;
};
DepthStencilState DepthEnabling
{
	DepthEnable = TRUE;
};
//]";

        private const string ArrayInitializerPreview = @"
//[
int arrayVariable1[2] = {
    1, 2
};
int arrayVariable2[2] =
{
    1, 2
};
//]";

        private const string ElsePreview = @"
void main()
{
//[
    if (false) {
    } else {
    }
//]
}";

        public FormattingNewLinesViewModel(IServiceProvider serviceProvider, NewLinesOptions page)
            : base(serviceProvider)
        {
            AddOpenBraceGroup("openbracestypes", "Position of open braces for types", TypePreview,
                () => page.OpenBracePositionForTypes, v => page.OpenBracePositionForTypes = v);

            AddOpenBraceGroup("openbracestechniques", "Position of open braces for techniques and passes", TechniquePreview,
                () => page.OpenBracePositionForTechniquesAndPasses, v => page.OpenBracePositionForTechniquesAndPasses = v);

            AddOpenBraceGroup("openbracesfunctions", "Position of open braces for functions", FunctionPreview,
                () => page.OpenBracePositionForFunctions, v => page.OpenBracePositionForFunctions = v);

            AddOpenBraceGroup("openbracescontrolblocks", "Position of open braces for control blocks", ControlBlockPreview,
                () => page.OpenBracePositionForControlBlocks, v => page.OpenBracePositionForControlBlocks = v);

            AddOpenBraceGroup("openbracesstateblocks", "Position of open braces for state blocks", StateBlockPreview,
                () => page.OpenBracePositionForStateBlocks, v => page.OpenBracePositionForStateBlocks = v);

            AddOpenBraceGroup("openbracesarrayinitializers", "Position of open braces for array initializers", ArrayInitializerPreview,
                () => page.OpenBracePositionForArrayInitializers, v => page.OpenBracePositionForArrayInitializers = v);

            Items.Add(new TextBlock { Text = "New line options for keywords" });

            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Place \"else\" on new line", () => page.PlaceElseOnNewLine, v => page.PlaceElseOnNewLine = v), ElsePreview, this));
        }

        private void AddOpenBraceGroup(
            string groupName, 
            string groupDescription, 
            string preview,
            Func<OpenBracesPosition> getValue, 
            Action<OpenBracesPosition> setValue)
        {
            Items.Add(new TextBlock { Text = groupDescription });

            Items.Add(new RadioButtonViewModel<OpenBracesPosition>(preview, groupName, OpenBracesPosition.MoveToNewLine,
                new Option<OpenBracesPosition>("Move to a new line", getValue, setValue), this));
            Items.Add(new RadioButtonViewModel<OpenBracesPosition>(preview, groupName, OpenBracesPosition.KeepOnSameLineAndPrependSpace,
                new Option<OpenBracesPosition>("Keep on the same line, but add a space before", getValue, setValue), this));
            Items.Add(new RadioButtonViewModel<OpenBracesPosition>(preview, groupName, OpenBracesPosition.DoNotMove,
                new Option<OpenBracesPosition>("Don't automatically reposition", getValue, setValue), this));
        }
    }
}