using System;
using HlslTools.Formatting;
using HlslTools.VisualStudio.Options.ViewModels;
using HlslTools.VisualStudio.Options.Views;

namespace HlslTools.VisualStudio.Options
{
    internal sealed class HlslFormattingNewLinesOptionsPage : OptionsPageBase<NewLinesOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider)
        {
            return new OptionsPreviewControl(() => new FormattingNewLinesViewModel(serviceProvider, Options));
        }
    }
}