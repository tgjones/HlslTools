using System;
using ShaderTools.Editor.VisualStudio.Core.Options.Views;
using ShaderTools.Editor.VisualStudio.ShaderLab.Options.Views;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Options
{
    internal sealed class ShaderLabFormattingGeneralOptionsPage : ShaderLabOptionsPageBase<GeneralOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider) => new ShaderLabFormattingGeneralOptionsControl(Options);
    }

    internal class GeneralOptions
    {
        public bool AutomaticallyFormatBlockOnCloseBrace { get; set; } = true;
        public bool AutomaticallyFormatOnPaste { get; set; } = true;
    }
}