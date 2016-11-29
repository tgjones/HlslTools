using System;
using ShaderTools.Hlsl.Formatting;

namespace ShaderTools.VisualStudio.Hlsl.Options
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