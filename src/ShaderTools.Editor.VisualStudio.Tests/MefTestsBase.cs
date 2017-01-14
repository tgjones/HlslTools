using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using NUnit.Framework;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;

namespace ShaderTools.Editor.VisualStudio.Tests
{
    // Based partly on https://github.com/jaredpar/EditorUtils/blob/master/Src/EditorUtils/EditorHostFactory.cs
    internal abstract class MefTestsBase
    {
        private static readonly string[] EditorComponents =
        {
            // Core editor components
            "Microsoft.VisualStudio.Platform.VSEditor.dll",

            // Not entirely sure why this is suddenly needed
            "Microsoft.VisualStudio.Text.Internal.dll",

            // Must include this because several editor options are actually stored as exported information 
            // on this DLL.  Including most importantly, the tabsize information
            "Microsoft.VisualStudio.Text.Logic.dll",

            // Include this DLL to get several more EditorOptions including WordWrapStyle
            "Microsoft.VisualStudio.Text.UI.dll",

            // Include this DLL to get more EditorOptions values and the core editor
            "Microsoft.VisualStudio.Text.UI.Wpf.dll"
        };

        /// <summary>
        /// A list of key names for versions of Visual Studio which have the editor components 
        /// necessary to create an EditorHost instance.  Listed in preference order
        /// </summary>
        private static readonly string[] VisualStudioSkuKeyNames =
        {
            // Standard non-express SKU of Visual Studio
            "VisualStudio",

            // Windows Desktop express
            "WDExpress",

            // Visual C# express
            "VCSExpress",

            // Visual C++ express
            "VCExpress",

            // Visual Basic Express
            "VBExpress",
        };

        protected CompositionContainer Container { get; private set; }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            var editorCatalogs = GetEditorCatalogs(EditorVersion.Vs2017);
            var localCatalog = new DirectoryCatalog(".");
            var catalog = new AggregateCatalog(editorCatalogs.Union(new[] { localCatalog }));
            Container = new CompositionContainer(catalog, new UndoExportProvider());

            OnTestFixtureSetUp();
        }

        /// <summary>
        /// Load the list of editor assemblies into the specified catalog list.  This method will
        /// throw on failure
        /// </summary>
        private static IEnumerable<ComposablePartCatalog> GetEditorCatalogs(EditorVersion editorVersion)
        {
            string version;
            string installDirectory;
            if (!TryGetEditorInfo(editorVersion, out version, out installDirectory))
            {
                throw new Exception("Unable to calculate the version of Visual Studio installed on the machine");
            }

            if (!TryLoadInteropAssembly(installDirectory))
            {
                var message = string.Format("Unable to load the interop assemblies.  Install directory is: {0}", installDirectory);
                throw new Exception(message);
            }

            // Load the core editor compontents from the GAC
            var versionInfo = string.Format(", Version={0}, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL", version);
            foreach (var name in EditorComponents)
            {
                var simpleName = name.Substring(0, name.Length - 4);
                var qualifiedName = simpleName + versionInfo;

                Assembly assembly;
                try
                {
                    assembly = Assembly.Load(qualifiedName);
                }
                catch (Exception e)
                {
                    var msg = string.Format("Unable to load editor dependency {0}", name);
                    throw new Exception(msg, e);
                }

                yield return new AssemblyCatalog(assembly);
            }
        }

        private static bool TryGetEditorInfo(EditorVersion editorVersion, out string fullVersion, out string installDirectory)
        {
            var shortVersion = GetShortVersionString(editorVersion);
            return TryGetEditorInfoCore(shortVersion, out fullVersion, out installDirectory);
        }

        private static bool TryGetEditorInfoCore(string shortVersion, out string fullversion, out string installDirectory)
        {
            if (TryGetInstallDirectory(shortVersion, out installDirectory))
            {
                fullversion = string.Format("{0}.0.0", shortVersion);
                return true;
            }

            fullversion = null;
            return false;
        }

        /// <summary>
        /// Try and get the installation directory for the specified version of Visual Studio.  This 
        /// will fail if the specified version of Visual Studio isn't installed
        /// </summary>
        private static bool TryGetInstallDirectory(string shortVersion, out string installDirectory)
        {
            foreach (var skuKeyName in VisualStudioSkuKeyNames)
            {
                if (TryGetInstallDirectory(skuKeyName, shortVersion, out installDirectory))
                {
                    return true;
                }
            }

            installDirectory = null;
            return false;
        }

        /// <summary>
        /// Try and get the installation directory for the specified SKU of Visual Studio.  This 
        /// will fail if the specified version of Visual Studio isn't installed
        /// </summary>
        private static bool TryGetInstallDirectory(string skuKeyName, string shortVersion, out string installDirectory)
        {
            try
            {
                var subKeyPath = String.Format(@"Software\Microsoft\{0}\{1}", skuKeyName, shortVersion);
                using (var key = Registry.LocalMachine.OpenSubKey(subKeyPath, writable: false))
                {
                    if (key != null)
                    {
                        installDirectory = key.GetValue("InstallDir", null) as string;
                        if (!String.IsNullOrEmpty(installDirectory))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Ignore and try the next version
                Console.WriteLine("Error getting install directory for '{0}', '{1}': {2}", skuKeyName, shortVersion, ex);
            }

            installDirectory = null;
            return false;
        }

        /// <summary>
        /// The interop assembly isn't included in the GAC and it doesn't offer any MEF components (it's
        /// just a simple COM interop library).  Hence it needs to be loaded a bit specially.  Just find
        /// the assembly on disk and hook into the resolve event
        /// </summary>
        private static bool TryLoadInteropAssembly(string installDirectory)
        {
            const string interopName = "Microsoft.VisualStudio.Platform.VSEditor.Interop";
            const string interopNameWithExtension = interopName + ".dll";
            var interopAssemblyPath = Path.Combine(installDirectory, "PrivateAssemblies");
            interopAssemblyPath = Path.Combine(interopAssemblyPath, interopNameWithExtension);
            try
            {
                var interopAssembly = Assembly.LoadFrom(interopAssemblyPath);
                if (interopAssembly == null)
                {
                    return false;
                }

                var comparer = StringComparer.OrdinalIgnoreCase;
                AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
                {
                    if (comparer.Equals(e.Name, interopAssembly.FullName))
                    {
                        return interopAssembly;
                    }

                    return null;
                };

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static int GetVersionNumber(EditorVersion version)
        {
            switch (version)
            {
                case EditorVersion.Vs2010: return 10;
                case EditorVersion.Vs2012: return 11;
                case EditorVersion.Vs2013: return 12;
                case EditorVersion.Vs2015: return 14;
                case EditorVersion.Vs2017: return 15;
                default:
                    throw new Exception(string.Format("Unexpected enum value {0}", version));
            }
        }

        private static string GetShortVersionString(EditorVersion version)
        {
            var number = GetVersionNumber(version);
            return string.Format("{0}.0", number);
        }

        /// <summary>
        /// The supported list of editor versions 
        /// </summary>
        /// <remarks>These must be listed in ascending version order</remarks>
        private enum EditorVersion
        {
            Vs2010,
            Vs2012,
            Vs2013,
            Vs2015,
            Vs2017
        }

        protected virtual void OnTestFixtureSetUp()
        {

        }
    }
}