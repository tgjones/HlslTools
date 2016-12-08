using ShaderTools.VisualStudio.Core.Options;

namespace ShaderTools.VisualStudio.ShaderLab.Options
{
    internal abstract class ShaderLabOptionsPageBase<TOptions> : OptionsPageBase<IShaderLabOptionsService, TOptions>
        where TOptions : class, new()
    {
    }
}