using ShaderTools.VisualStudio.Core.Options;

namespace ShaderTools.VisualStudio.ShaderLab.Options
{
    internal interface IShaderLabOptionsService : IOptionsService
    {
        AdvancedOptions AdvancedOptions { get; }
        GeneralOptions GeneralOptions { get; }
    }
}