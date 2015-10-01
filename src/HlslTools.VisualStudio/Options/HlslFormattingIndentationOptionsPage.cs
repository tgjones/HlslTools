using System;
using HlslTools.Formatting;
using HlslTools.VisualStudio.Options.ViewModels;
using HlslTools.VisualStudio.Options.Views;

namespace HlslTools.VisualStudio.Options
{
    internal sealed class HlslFormattingIndentationOptionsPage : OptionsPageBase<IndentationOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider)
        {
            return new OptionsPreviewControl(() => new FormattingIndentationViewModel(serviceProvider, Options));
        }
    }
}