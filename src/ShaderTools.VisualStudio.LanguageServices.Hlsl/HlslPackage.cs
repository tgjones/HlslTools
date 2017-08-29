using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ShaderTools.CodeAnalysis;
using ShaderTools.VisualStudio.LanguageServices.Hlsl.LanguageService;
using ShaderTools.VisualStudio.LanguageServices.Hlsl.Options.Formatting;
using ShaderTools.VisualStudio.LanguageServices.LanguageService;
using ShaderTools.VisualStudio.LanguageServices.Registration;

namespace ShaderTools.VisualStudio.LanguageServices.Hlsl
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.HlslPackageIdString)]

    [ProvideLanguageService(typeof(HlslLanguageService), LanguageNames.Hlsl, 0,
        ShowCompletion = true,
        ShowSmartIndent = true,
        ShowDropDownOptions = true,
        EnableAdvancedMembersOption = true,
        DefaultToInsertSpaces = true,
        EnableLineNumbers = true,
        RequestStockColors = true)]

    [ProvideService(typeof(HlslLanguageService), ServiceName = "HLSL Language Service")]

    [ProvideLanguageEditorOptionPage(typeof(AdvancedOptionPage), LanguageNames.Hlsl, null, "Advanced", "120")]
    [ProvideLanguageEditorOptionPage(typeof(FormattingOptionPage), LanguageNames.Hlsl, "Formatting", "General", "123")]
    [ProvideLanguageEditorOptionPage(typeof(FormattingIndentationOptionPage), LanguageNames.Hlsl, "Formatting", "Indentation", "124")]
    [ProvideLanguageEditorOptionPage(typeof(FormattingNewLinesPage), LanguageNames.Hlsl, "Formatting", "New Lines", "125")]
    [ProvideLanguageEditorOptionPage(typeof(FormattingSpacingPage), LanguageNames.Hlsl, "Formatting", "Spacing", "126")]

    [ProvideLanguageExtension(typeof(HlslLanguageService), ".hlsl")]
    [ProvideLanguageExtension(typeof(HlslLanguageService), ".hlsli")]
    [ProvideLanguageExtension(typeof(HlslLanguageService), ".fx")]
    [ProvideLanguageExtension(typeof(HlslLanguageService), ".fxh")]
    [ProvideLanguageExtension(typeof(HlslLanguageService), ".vsh")]
    [ProvideLanguageExtension(typeof(HlslLanguageService), ".psh")]
    [ProvideLanguageExtension(typeof(HlslLanguageService), ".cginc")]
    [ProvideLanguageExtension(typeof(HlslLanguageService), ".compute")]
    [ProvideLanguageExtension(typeof(HlslLanguageService), ".shader")]

    // Adds support for user mapping of custom file extensions.
    [ProvideFileExtensionMapping(
        "{A95B1F48-2A2E-492C-BABE-8DCC8A4643A8}", 
        "HLSL Editor", 
        typeof(IVsEditorFactory), // A bit weird, but seems to work, and means we don't need to implement IVsEditorFactory ourselves.
        typeof(HlslLanguageService),
        Guids.HlslPackageIdString, 
        100)]

    [ProvideBraceCompletion(LanguageNames.Hlsl)]
    internal sealed class HlslPackage : AbstractPackage<HlslPackage, HlslLanguageService>
    {
        protected override string ShaderToolsLanguageName => LanguageNames.Hlsl;

        protected override HlslLanguageService CreateLanguageService()
        {
            return new HlslLanguageService(this);
        }
    }
}