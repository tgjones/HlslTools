using System;
using ShaderTools.VisualStudio.Core.Options.Views;
using ShaderTools.VisualStudio.ShaderLab.Options.Views;

namespace ShaderTools.VisualStudio.ShaderLab.Options
{
    internal sealed class ShaderLabAdvancedOptionsPage : ShaderLabOptionsPageBase<AdvancedOptions>
    {
        protected override OptionsControlBase CreateControl(IServiceProvider serviceProvider) => new ShaderLabAdvancedOptionsControl(Options);
    }

    internal class AdvancedOptions
    {
        // Outlining
        public bool EnterOutliningModeWhenFilesOpen { get; set; } = true;

        // IntelliSense
        public bool EnableIntelliSense { get; set; } = true;
        public bool EnableErrorReporting { get; set; } = true;
        public bool EnableSquiggles { get; set; } = true;
    }
}