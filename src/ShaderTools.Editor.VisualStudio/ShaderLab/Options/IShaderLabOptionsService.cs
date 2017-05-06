using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Options
{
    internal interface IShaderLabOptionsService : IOptionsService
    {
        AdvancedOptions AdvancedOptions { get; }
        GeneralOptions GeneralOptions { get; }
    }
}