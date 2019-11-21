using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using ShaderTools.CodeAnalysis.Editor;
using ShaderTools.CodeAnalysis.Editor.Implementation;
using ShaderTools.CodeAnalysis.Log;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.VisualStudio.LanguageServices.Classification;
using ShaderTools.VisualStudio.LanguageServices.ErrorList;
using ShaderTools.VisualStudio.LanguageServices.LanguageService;
using ShaderTools.VisualStudio.LanguageServices.Utilities;

namespace ShaderTools.VisualStudio.LanguageServices
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.ShaderToolsPackageIdString)]
    internal sealed class ShaderToolsPackage : AbstractPackage
    {
        // Updated by build process.
        public const string Version = "1.1.300";

        private VisualStudioWorkspace _workspace;

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

            Logger.SetLogger(new VisualStudioLogger(this));

            var componentModel = this.ComponentModel;
            _workspace = componentModel.GetService<VisualStudioWorkspace>();

            // Ensure the options persisters are loaded since we have to fetch options from the shell
            componentModel.GetExtensions<IOptionPersister>();

            LoadComponentsInUIContextOnceSolutionFullyLoaded();
        }

        protected override void LoadComponentsInUIContext()
        {
            // Force-load services that don't load themselves.
            ComponentModel.GetService<ThemeColorFixer>();
            ComponentModel.GetService<ErrorsTableDataSource>();
        }

        protected override void Dispose(bool disposing)
        {
            DisposeVisualStudioServices();

            base.Dispose(disposing);
        }

        private void DisposeVisualStudioServices()
        {
            if (_workspace != null)
            {
                var documentTrackingService = _workspace.Services.GetService<IDocumentTrackingService>() as VisualStudioDocumentTrackingService;
                documentTrackingService.Dispose();
            }
        }
    }
}
