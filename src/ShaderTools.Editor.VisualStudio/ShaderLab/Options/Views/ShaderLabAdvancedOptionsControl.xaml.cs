using ShaderTools.Editor.VisualStudio.Core.Options.Views;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Options.Views
{
    internal partial class ShaderLabAdvancedOptionsControl : OptionsControlBase
    {
        public ShaderLabAdvancedOptionsControl(AdvancedOptions page)
        {
            InitializeComponent();

            BindToOption(EnterOutliningModeWhenFilesOpenCheckBox, page, nameof(page.EnterOutliningModeWhenFilesOpen));
            BindToOption(EnableIntelliSenseCheckBox, page, nameof(page.EnableIntelliSense));
            BindToOption(EnableErrorReportingCheckBox, page, nameof(page.EnableErrorReporting));
            BindToOption(EnableSquigglesCheckBox, page, nameof(page.EnableSquiggles));
        }
    }
}
