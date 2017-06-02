using System;
using Microsoft.VisualStudio.Shell;

namespace ShaderTools.Editor.VisualStudio.Core.Util.Extensions
{
    internal static class ServiceProviderExtensions
    {
        public static SVsServiceProvider AsVsServiceProvider(this IServiceProvider sp)
        {
            return new VsServiceProviderWrapper(sp);
        }
    }
}