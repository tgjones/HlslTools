using System;
using HlslTools.Formatting;
using HlslTools.VisualStudio.Options;

namespace HlslTools.VisualStudio.Tests.Support
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