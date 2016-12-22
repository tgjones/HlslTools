using System;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.Editor.VisualStudio.Core.Options.Views;
using ShaderTools.Editor.VisualStudio.Hlsl.Options.ViewModels;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Options
{
    internal sealed class HlslFormattingSpacingOptionsPage : HlslOptionsPageBase<SpacingOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider)
        {
            return new OptionsPreviewControl(() => new FormattingSpacingViewModel(serviceProvider, Options));
        }
    }
}