using Microsoft.VisualStudio.Shell;

namespace ShaderTools.VisualStudio.Hlsl.Util.Extensions
{
    internal static class ServiceProviderExtensions
    {
        public static HlslToolsPackage GetHlslToolsService(this SVsServiceProvider serviceProvider)
        {
            return (HlslToolsPackage)serviceProvider.GetService(typeof(HlslToolsPackage));
        }
    }
}