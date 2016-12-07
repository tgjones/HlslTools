using Microsoft.VisualStudio.Shell;

namespace ShaderTools.VisualStudio.Hlsl.Util.Extensions
{
    internal static class ServiceProviderExtensions
    {
        public static HlslPackage GetHlslToolsService(this SVsServiceProvider serviceProvider)
        {
            return (HlslPackage)serviceProvider.GetService(typeof(HlslPackage));
        }
    }
}