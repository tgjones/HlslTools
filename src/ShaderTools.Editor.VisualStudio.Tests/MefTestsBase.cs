using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Setup.Configuration;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;

namespace ShaderTools.Editor.VisualStudio.Tests
{
    // Based partly on https://github.com/jaredpar/EditorUtils/blob/master/Src/EditorUtils/EditorHostFactory.cs
    internal abstract class MefTestsBase
    {
        private static readonly string[] LocalEditorComponents =
        {
            // Must include this because several editor options are actually stored as exported information 
            // on this DLL.  Including most importantly, the tabsize information
            "Microsoft.VisualStudio.Text.Logic",

            // Include this DLL to get several more EditorOptions including WordWrapStyle
            "Microsoft.VisualStudio.Text.UI",

            // Include this DLL to get more EditorOptions values and the core editor
            "Microsoft.VisualStudio.Text.UI.Wpf"
        };

        private static readonly string[] PrivateEditorComponents =
        {
            // Core editor components
            @"Common7\IDE\CommonExtensions\Microsoft\Editor\Microsoft.VisualStudio.Platform.VSEditor.dll",

            // Not entirely sure why this is suddenly needed
            @"Common7\IDE\PrivateAssemblies\Microsoft.VisualStudio.Text.Internal.dll",

            // Must include this because several editor options are actually stored as exported information 
            // on this DLL.  Including most importantly, the tabsize information
            @"Common7\IDE\CommonExtensions\Microsoft\Editor\Microsoft.VisualStudio.Text.Logic.dll",

            // Include this DLL to get several more EditorOptions including WordWrapStyle
            @"Common7\IDE\CommonExtensions\Microsoft\Editor\Microsoft.VisualStudio.Text.UI.dll",

            // Include this DLL to get more EditorOptions values and the core editor
            @"Common7\IDE\CommonExtensions\Microsoft\Editor\Microsoft.VisualStudio.Text.UI.Wpf.dll"
        };

        protected CompositionContainer Container { get; private set; }

        protected MefTestsBase()
        {
            var editorCatalogs = GetEditorCatalogs();
            var localCatalog = new DirectoryCatalog(".");
            var catalog = new AggregateCatalog(editorCatalogs.Union(new[] { localCatalog }));
            Container = new CompositionContainer(catalog, new UndoExportProvider());

            OnTestFixtureSetUp();
        }

        /// <summary>
        /// Load the list of editor assemblies into the specified catalog list.  This method will
        /// throw on failure
        /// </summary>
        private static IEnumerable<ComposablePartCatalog> GetEditorCatalogs()
        {
            if (!TryGetInstallDirectory(out string installedVersion, out string installDirectory))
            {
                throw new Exception("Unable to calculate the version of Visual Studio installed on the machine");
            }

            LoadInteropAssemblies(installDirectory);

            // Load the locally referenced editor compontents.
            foreach (var name in LocalEditorComponents)
            {
                var assembly = Assembly.Load(name);
                yield return new AssemblyCatalog(assembly);
            }

            // Load the core editor compontents.
            foreach (var name in PrivateEditorComponents)
            {
                var fullPath = Path.Combine(installDirectory, name);
                var assembly = Assembly.LoadFrom(fullPath);
                yield return new AssemblyCatalog(assembly);
            }
        }

        /// <summary>
        /// Try and get the installation directory for any version of Visual Studio 2017+.
        /// </summary>
        private static bool TryGetInstallDirectory(out string installedVersion, out string installDirectory)
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

                        installedVersion = instance.GetInstallationVersion();
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

            installedVersion = null;
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

        /// <summary>
        /// This is for assemblies that don't offer any MEF components (i.e. just a simple COM interop library).
        /// </summary>
        private static void LoadInteropAssemblies(string installDirectory)
        {
            var interopAssemblies = new string[]
            {
                "Microsoft.VisualStudio.Platform.VSEditor.Interop.dll",
                "Microsoft.VisualStudio.Threading.dll"
            };

            var loadedAssemblies = interopAssemblies
                .Select(x =>
                {
                    var interopAssemblyPath = Path.Combine(installDirectory, "Common7", "IDE", "PrivateAssemblies", x);
                    var interopAssembly = Assembly.LoadFrom(interopAssemblyPath);
                    if (interopAssembly == null)
                        throw new Exception($"Unable to load interop assembly: {x}");
                    return interopAssembly;
                })
                .ToDictionary(x => x.FullName, x => x, StringComparer.OrdinalIgnoreCase);

            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                if (loadedAssemblies.TryGetValue(e.Name, out Assembly assembly))
                    return assembly;
                return null;
            };
        }

        protected virtual void OnTestFixtureSetUp()
        {

        }
    }
}