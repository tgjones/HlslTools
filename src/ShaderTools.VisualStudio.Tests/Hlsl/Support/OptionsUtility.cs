using System;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.VisualStudio.Core.Options;
using ShaderTools.VisualStudio.Hlsl.Options;

namespace ShaderTools.VisualStudio.Tests.Hlsl.Support
{
    internal class FakeOptionsService : IHlslOptionsService
    {
        public event EventHandler OptionsChanged;

        public void RaiseOptionsChanged()
        {
            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }

        bool IOptionsService.EnableErrorReporting => AdvancedOptions.EnableErrorReporting;
        bool IOptionsService.EnableSquiggles => AdvancedOptions.EnableSquiggles;

        public AdvancedOptions AdvancedOptions { get; } = new AdvancedOptions();
        public GeneralOptions GeneralOptions { get; } = new GeneralOptions();
        public FormattingOptions FormattingOptions { get; } = new FormattingOptions();
    }
}