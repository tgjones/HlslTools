using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ShaderTools.CodeAnalysis;
using ShaderTools.VisualStudio.LanguageServices.Registration;
using ShaderTools.VisualStudio.LanguageServices.LanguageService;
using ShaderTools.VisualStudio.LanguageServices.ShaderLab.LanguageService;

namespace ShaderTools.VisualStudio.LanguageServices.ShaderLab
{
    //[PackageRegistration(UseManagedResourcesOnly = true)]
    //[Guid(Guids.ShaderLabPackageIdString)]

    //[ProvideLanguageService(typeof(ShaderLabLanguageService), LanguageNames.ShaderLab, 0,
    //    ShowCompletion = true,
    //    ShowSmartIndent = true,
    //    ShowDropDownOptions = true,
    //    EnableAdvancedMembersOption = true,
    //    DefaultToInsertSpaces = true,
    //    EnableLineNumbers = true,
    //    RequestStockColors = true)]

    //[ProvideService(typeof(ShaderLabLanguageService), ServiceName = "ShaderLab Language Service")]

    ////[ProvideLanguageEditorOptionPage(typeof(ShaderLabAdvancedOptionsPage), ShaderLabConstants.LanguageName, null, "Advanced", "120")]
    ////[ProvideLanguageEditorOptionPage(typeof(ShaderLabFormattingGeneralOptionsPage), ShaderLabConstants.LanguageName, null, "Formatting", "122")]

    //[ProvideLanguageExtension(typeof(ShaderLabLanguageService), ".shader")]

    //[ProvideFileExtensionMapping(
    //    "{C911385A-6AF2-4C17-B1FE-4D29F6E58B31}",
    //    "ShaderLab Editor",
    //    typeof(IVsEditorFactory), // A bit weird, but seems to work, and means we don't need to implement IVsEditorFactory ourselves.
    //    typeof(ShaderLabLanguageService),
    //    Guids.ShaderLabPackageIdString,
    //    100)]

    //[ProvideBraceCompletion(LanguageNames.ShaderLab)]
    internal sealed class ShaderLabPackage : AbstractPackage<ShaderLabPackage, ShaderLabLanguageService>
    {
        protected override string ShaderToolsLanguageName => LanguageNames.ShaderLab;

        protected override ShaderLabLanguageService CreateLanguageService()
        {
            return new ShaderLabLanguageService(this);
        }
    }
}
