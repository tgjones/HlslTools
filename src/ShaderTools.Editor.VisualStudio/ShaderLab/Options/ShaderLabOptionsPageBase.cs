using ShaderTools.Editor.VisualStudio.Core.Options;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Options
{
    internal abstract class ShaderLabOptionsPageBase<TOptions> : OptionsPageBase<IShaderLabOptionsService, TOptions>
        where TOptions : class, new()
    {
    }
}