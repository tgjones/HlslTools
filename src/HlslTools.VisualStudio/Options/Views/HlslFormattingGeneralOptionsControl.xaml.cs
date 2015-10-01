namespace HlslTools.VisualStudio.Options.Views
{
    internal partial class HlslFormattingGeneralOptionsControl : OptionsControlBase
    {
        public HlslFormattingGeneralOptionsControl(GeneralOptions page)
        {
            InitializeComponent();

            BindToOption(FormatOnSemicolonCheckBox, page, nameof(page.AutomaticallyFormatStatementOnSemicolon));
            BindToOption(FormatOnCloseBraceCheckBox, page, nameof(page.AutomaticallyFormatBlockOnCloseBrace));
            BindToOption(FormatOnPasteCheckBox, page, nameof(page.AutomaticallyFormatOnPaste));
        }
    }
}
