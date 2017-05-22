using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using ShaderTools.CodeAnalysis.Editor;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.VisualStudio.LanguageServices.Classification;
using ShaderTools.VisualStudio.LanguageServices.ErrorList;

namespace ShaderTools.VisualStudio.LanguageServices
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.ShaderToolsPackageIdString)]
    internal sealed class ShaderToolsPackage : Package
    {
        // Updated by build process.
        public const string Version = "1.0.0";

        private IComponentModel _componentModel;

        private IComponentModel ComponentModel
        {
            get
            {
                if (_componentModel == null)
                {
                    _componentModel = (IComponentModel) GetService(typeof(SComponentModel));
                }

                return _componentModel;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Ensure the options persisters are loaded since we have to fetch options from the shell
            ComponentModel.GetExtensions<IOptionPersister>();

            // Force-load services that don't load themselves.
            ComponentModel.GetService<ThemeColorFixer>();
            ComponentModel.GetService<ErrorsTableDataSource>();

            System.Threading.Tasks.Task.Run(() => LoadComponentsBackground());
        }

        private void LoadComponentsBackground()
        {
            // Perf: Initialize the command handlers.
            var commandHandlerServiceFactory = ComponentModel.GetService<ICommandHandlerServiceFactory>();
            commandHandlerServiceFactory.Initialize(ContentTypeNames.ShaderToolsContentType);
        }
    }
}
