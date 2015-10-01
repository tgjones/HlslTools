using System;
using HlslTools.VisualStudio.Options.Views;

namespace HlslTools.VisualStudio.Options
{
    internal sealed class HlslFormattingGeneralOptionsPage : OptionsPageBase<GeneralOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider) => new HlslFormattingGeneralOptionsControl(Options);
    }

    internal class GeneralOptions
    {
        public bool AutomaticallyFormatStatementOnSemicolon { get; set; } = true;
        public bool AutomaticallyFormatBlockOnCloseBrace { get; set; } = true;
        public bool AutomaticallyFormatOnPaste { get; set; } = true;
    }
}