using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using ShaderTools.VisualStudio.LanguageServices;

namespace ShaderTools.Editor.VisualStudio
{
    [InstalledProductRegistration("#110", "#112", ShaderToolsPackage.Version, IconResourceID = 400)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid("5A45E451-3706-4C2C-B472-AC569CEB7F43")]
    internal sealed class SetupPackage : Package
    {
    }
}
