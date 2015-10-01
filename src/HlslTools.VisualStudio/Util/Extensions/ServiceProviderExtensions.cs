using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace HlslTools.VisualStudio.Util.Extensions
{
    internal static class ServiceProviderExtensions
    {
        public static TServiceInterface GetService<TServiceClass, TServiceInterface>(this IServiceProvider sp)
        {
            return (TServiceInterface)sp.GetService(typeof(TServiceClass));
        }

        public static IComponentModel GetComponentModel(this SVsServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<SComponentModel, IComponentModel>();
        }

        public static HlslToolsPackage GetHlslToolsService(this SVsServiceProvider serviceProvider)
        {
            return (HlslToolsPackage) serviceProvider.GetService(typeof(HlslToolsPackage));
        }

        public static IVsShell GetShell(this SVsServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<SVsShell, IVsShell>();
        }

        public static SVsServiceProvider AsVsServiceProvider(this IServiceProvider sp)
        {
            return new VsServiceProviderWrapper(sp);
        }

        public static TInterface GetGlobalService<TService, TInterface>()
            where TService : class
            where TInterface : class
        {
            var serviceProvider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)Package.GetGlobalService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider));
            var guidService = typeof(TService).GUID;

            Guid riid = VSConstants.IID_IUnknown;
            IntPtr ppvObject = IntPtr.Zero;
            object obj = null;
            if (serviceProvider.QueryService(ref guidService, ref riid, out ppvObject) == 0)
            {
                if (ppvObject != IntPtr.Zero)
                {
                    try
                    {
                        obj = Marshal.GetObjectForIUnknown(ppvObject);
                    }
                    finally
                    {
                        Marshal.Release(ppvObject);
                    }
                }
            }
            return obj as TInterface;
        }
    }
}