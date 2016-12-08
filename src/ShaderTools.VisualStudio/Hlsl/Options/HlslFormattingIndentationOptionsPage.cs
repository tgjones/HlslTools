using System;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.VisualStudio.Core.Options.Views;
using ShaderTools.VisualStudio.Hlsl.Options.ViewModels;

namespace ShaderTools.VisualStudio.Hlsl.Options
{
    internal sealed class HlslFormattingIndentationOptionsPage : HlslOptionsPageBase<IndentationOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider)
        {
            return new OptionsPreviewControl(() => new FormattingIndentationViewModel(serviceProvider, Options));
        }
    }
}