using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.Editor.VisualStudio.Core;
using ShaderTools.Editor.VisualStudio.Core.Navigation;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;
using ShaderTools.Editor.VisualStudio.Hlsl.Navigation;
using ShaderTools.VisualStudio.LanguageServices.Hlsl.Options.Formatting;
using ShaderTools.VisualStudio.LanguageServices.Registration;

namespace ShaderTools.Editor.VisualStudio.Hlsl
{
    [InstalledProductRegistration("#110", "#112", Version, IconResourceID = 400)]

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(PackageId)]

    [ProvideLanguageService(typeof(HlslLanguageInfo), HlslConstants.LanguageName, 0,
        ShowCompletion = true,
        ShowSmartIndent = true,
        ShowDropDownOptions = true,
        EnableAdvancedMembersOption = true,
        DefaultToInsertSpaces = true,
        EnableLineNumbers = true,
        RequestStockColors = true)]

    [ProvideService(typeof(HlslLanguageInfo), ServiceName = "HLSL Language Service")]

    [ProvideLanguageEditorOptionPage(typeof(AdvancedOptionPage), HlslConstants.LanguageName, null, "Advanced", "120")]
    [ProvideLanguageEditorOptionPage(typeof(FormattingOptionPage), HlslConstants.LanguageName, "Formatting", "General", "123")]
    [ProvideLanguageEditorOptionPage(typeof(FormattingIndentationOptionPage), HlslConstants.LanguageName, "Formatting", "Indentation", "124")]
    [ProvideLanguageEditorOptionPage(typeof(FormattingNewLinesPage), HlslConstants.LanguageName, "Formatting", "New Lines", "125")]
    [ProvideLanguageEditorOptionPage(typeof(FormattingSpacingPage), HlslConstants.LanguageName, "Formatting", "Spacing", "126")]

    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension1)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension2)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension3)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension4)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension5)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension6)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension7)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension8)]

    // Adds support for user mapping of custom file extensions.
    [ProvideFileExtensionMapping(
        "{A95B1F48-2A2E-492C-BABE-8DCC8A4643A8}", 
        "HLSL Editor", 
        typeof(IVsEditorFactory),  // A bit weird, but seems to work, and means we don't need to implement IVsEditorFactory ourselves.
        typeof(HlslLanguageInfo), 
        PackageId, 
        100)]

    [ProvideBraceCompletion(HlslConstants.LanguageName)]
    internal sealed class HlslPackage : LanguagePackageBase
    {
        // Updated by build process.
        public const string Version = "1.0.0";

        private const string PackageId = "0E01DDB3-F537-4C49-9B50-BDA9DCCE2172";

        public static HlslPackage Instance { get; private set; }

        protected override CodeWindowManagerBase CreateCodeWindowManager(IVsCodeWindow window)
        {
            return new CodeWindowManager(this, window, this.AsVsServiceProvider());
        }

        protected override LanguageInfoBase CreateLanguageInfo()
        {
            return new HlslLanguageInfo(this);
        }

        protected override void Initialize()
        {
            Instance = this;        

            base.Initialize();
        }
    }
}