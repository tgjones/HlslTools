using System;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.Editor.VisualStudio.Core.Options;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Options
{
    internal interface IHlslOptionsService : IOptionsService
    {
        AdvancedOptions AdvancedOptions { get; }
        GeneralOptions GeneralOptions { get; }
        FormattingOptions FormattingOptions { get; }
    }
}