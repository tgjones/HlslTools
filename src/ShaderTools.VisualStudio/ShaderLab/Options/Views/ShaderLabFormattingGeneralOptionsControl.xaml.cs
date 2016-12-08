using ShaderTools.VisualStudio.Core.Options.Views;

namespace ShaderTools.VisualStudio.ShaderLab.Options.Views
{
    internal partial class ShaderLabFormattingGeneralOptionsControl : OptionsControlBase
    {
        public ShaderLabFormattingGeneralOptionsControl(GeneralOptions page)
        {
            InitializeComponent();

            BindToOption(FormatOnCloseBraceCheckBox, page, nameof(page.AutomaticallyFormatBlockOnCloseBrace));
            BindToOption(FormatOnPasteCheckBox, page, nameof(page.AutomaticallyFormatOnPaste));
        }
    }
}
