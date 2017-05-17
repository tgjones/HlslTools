using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
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

        protected override void Initialize()
        {
            base.Initialize();

            // Ensure the options persisters are loaded since we have to fetch options from the shell
            var componentModel = (IComponentModel) GetService(typeof(SComponentModel));
            componentModel.GetExtensions<IOptionPersister>();

            // Force-load services that don't load themselves.
            componentModel.GetService<ThemeColorFixer>();
            componentModel.GetService<ErrorsTableDataSource>();
        }
    }
}
