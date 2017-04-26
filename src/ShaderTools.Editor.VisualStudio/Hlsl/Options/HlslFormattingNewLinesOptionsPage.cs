using System;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.Editor.VisualStudio.Core.Options.Views;
using ShaderTools.Editor.VisualStudio.Hlsl.Options.ViewModels;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Options
{
    internal sealed class HlslFormattingNewLinesOptionsPage : HlslOptionsPageBase<NewLinesOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider)
        {
            return new OptionsPreviewControl(() => new FormattingNewLinesViewModel(serviceProvider, Options));
        }
    }
}