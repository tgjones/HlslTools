using System;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.VisualStudio.Hlsl.Options.ViewModels;
using ShaderTools.VisualStudio.Hlsl.Options.Views;

namespace ShaderTools.VisualStudio.Hlsl.Options
{
    internal sealed class HlslFormattingIndentationOptionsPage : OptionsPageBase<IndentationOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider)
        {
            return new OptionsPreviewControl(() => new FormattingIndentationViewModel(serviceProvider, Options));
        }
    }
}