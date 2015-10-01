using System;
using System.Windows.Controls;
using HlslTools.Formatting;

namespace HlslTools.VisualStudio.Options.ViewModels
{
    internal sealed class FormattingNewLinesViewModel : OptionsPreviewViewModelBase
    {
        private const string TypePreview = @"
//[
struct S {
    float4 position;
    float3 normal;
};
//]";

        private const string TechniquePreview = @"
//[
technique T {
    pass P {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
//]";

        private const string FunctionPreview = @"
//[
float4 main() {
    return float4(1, 0, 0, 1);
}
//]";

        private const string ControlBlockPreview = @"
void main()
{
//[
    for (int i = 0; i < 10; i++) {
    }
//]
}";

        private const string StateBlockPreview = @"
//[
DepthStencilState DepthDisabling {
	DepthEnable = FALSE;
	DepthWriteMask = ZERO;
};
//]";

        private const string ArrayInitializerPreview = @"
//[
int arrayVariable[2] = {
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
            Items.Add(new TextBlock { Text = "New line options for braces" });

            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Place open brace on new line for types", () => page.PlaceOpenBraceOnNewLineForTypes, v => page.PlaceOpenBraceOnNewLineForTypes = v), TypePreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Place open brace on new line for techniques and passes", () => page.PlaceOpenBraceOnNewLineForTechniquesAndPasses, v => page.PlaceOpenBraceOnNewLineForTechniquesAndPasses = v), TechniquePreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Place open brace on new line for functions", () => page.PlaceOpenBraceOnNewLineForFunctions, v => page.PlaceOpenBraceOnNewLineForFunctions = v), FunctionPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Place open brace on new line for control blocks", () => page.PlaceOpenBraceOnNewLineForControlBlocks, v => page.PlaceOpenBraceOnNewLineForControlBlocks = v), ControlBlockPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Place open brace on new line for state blocks", () => page.PlaceOpenBraceOnNewLineForStateBlocks, v => page.PlaceOpenBraceOnNewLineForStateBlocks = v), StateBlockPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Place open brace on new line for array initializers", () => page.PlaceOpenBraceOnNewLineForArrayInitializers, v => page.PlaceOpenBraceOnNewLineForArrayInitializers = v), ArrayInitializerPreview, this));

            Items.Add(new TextBlock { Text = "New line options for keywords" });

            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Place \"else\" on new line", () => page.PlaceElseOnNewLine, v => page.PlaceElseOnNewLine = v), ElsePreview, this));
        }
    }
}