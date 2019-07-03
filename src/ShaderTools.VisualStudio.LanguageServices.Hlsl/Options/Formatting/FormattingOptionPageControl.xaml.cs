using System;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.VisualStudio.LanguageServices.Options.UI;

namespace ShaderTools.VisualStudio.LanguageServices.Hlsl.Options.Formatting
{
    public partial class FormattingOptionPageControl : AbstractOptionPageControl
    {
        public FormattingOptionPageControl(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();

            BindToOption(FormatWhenTypingCheckBox, FeatureOnOffOptions.AutoFormattingOnTyping, LanguageNames.Hlsl);
            BindToOption(FormatOnCloseBraceCheckBox, FeatureOnOffOptions.AutoFormattingOnCloseBrace, LanguageNames.Hlsl);
            BindToOption(FormatOnCloseParenCheckBox, FeatureOnOffOptions.AutoFormattingOnCloseParen, LanguageNames.Hlsl);
            BindToOption(FormatOnSemicolonCheckBox, FeatureOnOffOptions.AutoFormattingOnSemicolon, LanguageNames.Hlsl);
            BindToOption(FormatOnPasteCheckBox, FeatureOnOffOptions.FormatOnPaste, LanguageNames.Hlsl);
        }
    }
}
