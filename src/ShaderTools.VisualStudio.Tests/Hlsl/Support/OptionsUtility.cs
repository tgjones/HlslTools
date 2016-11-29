using System;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.VisualStudio.Hlsl.Options;

namespace ShaderTools.VisualStudio.Tests.Hlsl.Support
{
    internal class FakeOptionsService : IOptionsService
    {
        public event EventHandler OptionsChanged;

        public void RaiseOptionsChanged()
        {
            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }

        public AdvancedOptions AdvancedOptions { get; } = new AdvancedOptions();
        public GeneralOptions GeneralOptions { get; } = new GeneralOptions();
        public FormattingOptions FormattingOptions { get; } = new FormattingOptions();
    }
}