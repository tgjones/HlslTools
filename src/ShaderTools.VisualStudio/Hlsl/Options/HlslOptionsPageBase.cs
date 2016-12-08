using ShaderTools.VisualStudio.Core.Options;

namespace ShaderTools.VisualStudio.Hlsl.Options
{
    internal abstract class HlslOptionsPageBase<TOptions> : OptionsPageBase<IHlslOptionsService, TOptions>
        where TOptions : class, new()
    {
    }
}