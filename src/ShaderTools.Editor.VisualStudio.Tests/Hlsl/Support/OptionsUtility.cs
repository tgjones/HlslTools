using System;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.Editor.VisualStudio.Hlsl.Options;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support
{
    internal class FakeOptionsService : IHlslOptionsService
    {
        public event EventHandler OptionsChanged;

        public void RaiseOptionsChanged()
        {
            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }

        bool IOptionsService.EnableIntelliSense => AdvancedOptions.EnableIntelliSense;
        bool IOptionsService.EnableErrorReporting => AdvancedOptions.EnableErrorReporting;
        bool IOptionsService.EnableSquiggles => AdvancedOptions.EnableSquiggles;

        public AdvancedOptions AdvancedOptions { get; } = new AdvancedOptions();
        public GeneralOptions GeneralOptions { get; } = new GeneralOptions();
        public FormattingOptions FormattingOptions { get; } = new FormattingOptions();
    }
}