using System;
using HlslTools.VisualStudio.Options.Views;

namespace HlslTools.VisualStudio.Options
{
    internal sealed class HlslAdvancedOptionsPage : OptionsPageBase<AdvancedOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider) => new HlslAdvancedOptionsControl(Options);
    }

    internal class AdvancedOptions
    {
        // Brace completion
        public bool AddSemicolonForTypes { get; set; } = true;

        // Outlining
        public bool EnterOutliningModeWhenFilesOpen { get; set; } = true;

        // IntelliSense
        public bool EnableErrorReporting { get; set; } = true;
        public bool EnableSquiggles { get; set; } = true;
    }
}