using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Options
{
    internal interface IHlslOptionsService : IOptionsService
    {
        AdvancedOptions AdvancedOptions { get; }
        GeneralOptions GeneralOptions { get; }
        FormattingOptions FormattingOptions { get; }
    }
}