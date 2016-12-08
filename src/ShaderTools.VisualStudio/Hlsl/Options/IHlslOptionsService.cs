using System;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.VisualStudio.Core.Options;

namespace ShaderTools.VisualStudio.Hlsl.Options
{
    internal interface IHlslOptionsService : IOptionsService
    {
        AdvancedOptions AdvancedOptions { get; }
        GeneralOptions GeneralOptions { get; }
        FormattingOptions FormattingOptions { get; }
    }
}