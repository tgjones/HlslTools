using System;
using HlslTools.Formatting;

namespace HlslTools.VisualStudio.Options
{
    internal interface IOptionsService
    {
        event EventHandler OptionsChanged;

        void RaiseOptionsChanged();

        AdvancedOptions AdvancedOptions { get; }
        GeneralOptions GeneralOptions { get; }
        FormattingOptions FormattingOptions { get; }
    }
}