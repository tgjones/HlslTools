using System;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.Editor.VisualStudio.Core.Options.Views;
using ShaderTools.Editor.VisualStudio.Hlsl.Options.ViewModels;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Options
{
    internal sealed class HlslFormattingIndentationOptionsPage : HlslOptionsPageBase<IndentationOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider)
        {
            return new OptionsPreviewControl(() => new FormattingIndentationViewModel(serviceProvider, Options));
        }
    }
}