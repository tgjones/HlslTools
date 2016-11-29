using System;
using System.Windows.Controls;
using ShaderTools.Hlsl.Formatting;

namespace ShaderTools.VisualStudio.Hlsl.Options.ViewModels
{
    internal sealed class FormattingIndentationViewModel : OptionsPreviewViewModelBase
    {
        private const string BlockContentPreview = @"
class C {
//[
    int Method() {
        int x;
        int y;
    }
//]
}";

        private const string IndentBracePreview = @"
class C {
//[
    int Method() {
        return 0;
    }
//]
}";

        private const string SwitchCasePreview = @"
class MyClass
{
    int Method(int foo){
//[
        switch (foo){
        case 2:
            break;
        }
//]
    }
}";

        private const string PreprocessorDirectivePreview = @"
float4 ReadTexture() { return float4(1, 0, 0, 1); }
//[
void main()
{
    #if TEXTURES
    float4 color = ReadTexture();
    #endif
}
//]";

        public FormattingIndentationViewModel(IServiceProvider serviceProvider, IndentationOptions page)
            : base(serviceProvider)
        {
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Indent block contents", () => page.IndentBlockContents, v => page.IndentBlockContents = v), BlockContentPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Indent open and close braces", () => page.IndentOpenAndCloseBraces, v => page.IndentOpenAndCloseBraces = v), IndentBracePreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Indent case contents", () => page.IndentCaseContents, v => page.IndentCaseContents = v), SwitchCasePreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Indent case labels", () => page.IndentCaseLabels, v => page.IndentCaseLabels = v), SwitchCasePreview, this));

            Items.Add(new TextBlock { Text = "Preprocessor directive indentation" });

            Items.Add(new RadioButtonViewModel<PreprocessorDirectivePosition>(PreprocessorDirectivePreview, "directive", PreprocessorDirectivePosition.OneIndentToLeft,
                new Option<PreprocessorDirectivePosition>("One indent to the left", () => page.PreprocessorDirectivePosition, v => page.PreprocessorDirectivePosition = v), this));
            Items.Add(new RadioButtonViewModel<PreprocessorDirectivePosition>(PreprocessorDirectivePreview, "directive", PreprocessorDirectivePosition.MoveToLeftmostColumn,
                new Option<PreprocessorDirectivePosition>("Move to the leftmost column", () => page.PreprocessorDirectivePosition, v => page.PreprocessorDirectivePosition = v), this));
            Items.Add(new RadioButtonViewModel<PreprocessorDirectivePosition>(PreprocessorDirectivePreview, "directive", PreprocessorDirectivePosition.LeaveIndented,
                new Option<PreprocessorDirectivePosition>("Leave indented", () => page.PreprocessorDirectivePosition, v => page.PreprocessorDirectivePosition = v), this));
        }
    }
}