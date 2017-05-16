using System;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Editor.Options;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.VisualStudio.LanguageServices.Options.UI;

namespace ShaderTools.VisualStudio.LanguageServices.Hlsl.Options
{
    public partial class AdvancedOptionPageControl : AbstractOptionPageControl
    {
        public AdvancedOptionPageControl(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();

            BindToOption(AddSemicolonForTypesCheckBox, BraceCompletionOptions.AddSemicolonForTypes, LanguageNames.Hlsl);
            BindToOption(EnterOutliningModeWhenFilesOpenCheckBox, FeatureOnOffOptions.Outlining, LanguageNames.Hlsl);
            BindToOption(EnableIntelliSenseCheckBox, FeatureOnOffOptions.IntelliSense, LanguageNames.Hlsl);
            BindToOption(EnableErrorReportingCheckBox, DiagnosticsOptions.EnableErrorReporting, LanguageNames.Hlsl);
            BindToOption(EnableSquigglesCheckBox, DiagnosticsOptions.EnableSquiggles, LanguageNames.Hlsl);
        }
    }
}
