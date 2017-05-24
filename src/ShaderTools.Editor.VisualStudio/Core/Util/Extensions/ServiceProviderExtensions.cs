using System;
using Microsoft.VisualStudio.Shell;

namespace ShaderTools.Editor.VisualStudio.Core.Util.Extensions
{
    internal static class ServiceProviderExtensions
    {
        public static TServiceInterface GetService<TServiceClass, TServiceInterface>(this IServiceProvider sp)
        {
            return (TServiceInterface)sp.GetService(typeof(TServiceClass));
        }

        public static SVsServiceProvider AsVsServiceProvider(this IServiceProvider sp)
        {
            return new VsServiceProviderWrapper(sp);
        }
    }
}