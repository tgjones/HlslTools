using System;
using ShaderTools.VisualStudio.Core.Options.Views;
using ShaderTools.VisualStudio.Hlsl.Options.Views;

namespace ShaderTools.VisualStudio.Hlsl.Options
{
    internal sealed class HlslAdvancedOptionsPage : HlslOptionsPageBase<AdvancedOptions>
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
        public bool EnableIntelliSense { get; set; } = true;
        public bool EnableErrorReporting { get; set; } = true;
        public bool EnableSquiggles { get; set; } = true;
    }
}