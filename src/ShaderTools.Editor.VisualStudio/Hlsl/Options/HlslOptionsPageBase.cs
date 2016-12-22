using ShaderTools.Editor.VisualStudio.Core.Options;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Options
{
    internal abstract class HlslOptionsPageBase<TOptions> : OptionsPageBase<IHlslOptionsService, TOptions>
        where TOptions : class, new()
    {
    }
}