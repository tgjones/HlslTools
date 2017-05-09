using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using ShaderTools.Editor.VisualStudio.Hlsl;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;

namespace ShaderTools.Editor.VisualStudio.Tests
{
    public abstract class MefTestsBase : VisualStudioTestsBase
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

        protected CompositionContainer Container { get; private set; }

        protected MefTestsBase()
        {
            var editorCatalogs = GetEditorCatalogs();
            var localCatalog = new AssemblyCatalog(typeof(HlslPackage).Assembly);
            var catalog = new AggregateCatalog(editorCatalogs.Union(new[] { localCatalog }));
            Container = new CompositionContainer(catalog, new UndoExportProvider());
        }

        /// <summary>
        /// Load the list of editor assemblies into the specified catalog list.  This method will
        /// throw on failure
        /// </summary>
        private IEnumerable<ComposablePartCatalog> GetEditorCatalogs()
        {
            // Load the core editor compontents from the GAC
            var versionInfo = ", Version=15.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL";

            // Load the locally referenced editor compontents.
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
    }
}