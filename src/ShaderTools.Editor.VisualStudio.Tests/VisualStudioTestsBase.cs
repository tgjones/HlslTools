using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Setup.Configuration;

namespace ShaderTools.Editor.VisualStudio.Tests
{
    public abstract class VisualStudioTestsBase : IDisposable
    {
        private static readonly string[] VisualStudioAssemblyDirectories =
        {
            @"Common7\IDE",
            @"Common7\IDE\PublicAssemblies",
            @"Common7\IDE\PrivateAssemblies",
            @"Common7\IDE\CommonExtensions\Microsoft\Editor",
        };

        protected string VisualStudioInstallationDirectory { get; }

        protected VisualStudioTestsBase()
        {
            string installationDirectory;
            if (!TryGetInstallDirectory(out installationDirectory))
                throw new Exception("Unable to calculate the version of Visual Studio installed on the machine");
            VisualStudioInstallationDirectory = installationDirectory;

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var requestedAssemblyFileName = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";

            foreach (var directory in VisualStudioAssemblyDirectories)
            {
                var fullPath = Path.Combine(VisualStudioInstallationDirectory, directory, requestedAssemblyFileName);
                if (File.Exists(fullPath))
                    return Assembly.LoadFrom(fullPath);
            }

            return null;
        }

        public virtual void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
        }

        /// <summary>
        /// Try and get the installation directory for any version of Visual Studio 2017+.
        /// </summary>
        private static bool TryGetInstallDirectory(out string installDirectory)
        {
            try
            {
                var query = GetQuery();
                var query2 = (ISetupConfiguration2) query;
                var e = query2.EnumAllInstances();

                var helper = (ISetupHelper) query;

                int fetched;
                var instances = new ISetupInstance[1];
                do
                {
                    e.Next(1, instances, out fetched);
                    if (fetched > 0)
                    {
                        var instance = instances[0];
                        installDirectory = instance.GetInstallationPath();
                        return true;
                    }
                }
                while (fetched > 0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error 0x{ex.HResult:x8}: {ex.Message}");
            }

            installDirectory = null;
            return false;
        }

        private const int REGDB_E_CLASSNOTREG = unchecked((int) 0x80040154);

        private static ISetupConfiguration GetQuery()
        {
            try
            {
                // Try to CoCreate the class object. 
                return new SetupConfiguration();
            }
            catch (COMException ex) when (ex.HResult == REGDB_E_CLASSNOTREG)
            {
                // Try to get the class object using app-local call. 
                ISetupConfiguration query;
                var result = GetSetupConfiguration(out query, IntPtr.Zero);

                if (result < 0)
                {
                    throw new COMException("Failed to get query", result);
                }

                return query;
            }
        }

        [DllImport("Microsoft.VisualStudio.Setup.Configuration.Native.dll", ExactSpelling = true, PreserveSig = true)]
        private static extern int GetSetupConfiguration(
            [MarshalAs(UnmanagedType.Interface), Out]out ISetupConfiguration configuration,
            IntPtr reserved);
    }
}