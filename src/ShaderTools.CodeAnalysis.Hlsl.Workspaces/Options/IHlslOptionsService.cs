using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.CodeAnalysis.Hlsl.Options
{
    internal interface IHlslOptionsService
    {
        FormattingOptions GetPrimaryWorkspaceFormattingOptions();
        FormattingOptions GetFormattingOptions(OptionSet options);
    }
}