namespace ShaderTools.VisualStudio.Hlsl.Options.Views
{
    internal partial class HlslAdvancedOptionsControl : OptionsControlBase
    {
        public HlslAdvancedOptionsControl(AdvancedOptions page)
        {
            InitializeComponent();

            BindToOption(AddSemicolonForTypesCheckBox, page, nameof(page.AddSemicolonForTypes));
            BindToOption(EnterOutliningModeWhenFilesOpenCheckBox, page, nameof(page.EnterOutliningModeWhenFilesOpen));
            BindToOption(EnableIntelliSenseCheckBox, page, nameof(page.EnableIntelliSense));
            BindToOption(EnableErrorReportingCheckBox, page, nameof(page.EnableErrorReporting));
            BindToOption(EnableSquigglesCheckBox, page, nameof(page.EnableSquiggles));
        }
    }
}
