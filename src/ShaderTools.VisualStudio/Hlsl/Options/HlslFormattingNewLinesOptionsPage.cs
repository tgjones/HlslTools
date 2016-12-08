using System;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.VisualStudio.Core.Options.Views;
using ShaderTools.VisualStudio.Hlsl.Options.ViewModels;

namespace ShaderTools.VisualStudio.Hlsl.Options
{
    internal sealed class HlslFormattingNewLinesOptionsPage : HlslOptionsPageBase<NewLinesOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider)
        {
            return new OptionsPreviewControl(() => new FormattingNewLinesViewModel(serviceProvider, Options));
        }
    }
}