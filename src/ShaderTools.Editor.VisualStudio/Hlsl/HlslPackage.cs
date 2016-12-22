using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.Editor.VisualStudio.Core;
using ShaderTools.Editor.VisualStudio.Core.Navigation;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;
using ShaderTools.Editor.VisualStudio.Hlsl.Editing.BraceCompletion;
using ShaderTools.Editor.VisualStudio.Hlsl.Navigation;
using ShaderTools.Editor.VisualStudio.Hlsl.Options;
using ShaderTools.Editor.VisualStudio.Hlsl.SyntaxVisualizer;

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

    [ProvideLanguageEditorOptionPage(typeof(HlslAdvancedOptionsPage), HlslConstants.LanguageName, null, "Advanced", "120")]
    [ProvideLanguageEditorOptionPage(typeof(HlslFormattingGeneralOptionsPage), HlslConstants.LanguageName, "Formatting", "General", "123")]
    [ProvideLanguageEditorOptionPage(typeof(HlslFormattingIndentationOptionsPage), HlslConstants.LanguageName, "Formatting", "Indentation", "124")]
    [ProvideLanguageEditorOptionPage(typeof(HlslFormattingNewLinesOptionsPage), HlslConstants.LanguageName, "Formatting", "New Lines", "125")]
    [ProvideLanguageEditorOptionPage(typeof(HlslFormattingSpacingOptionsPage), HlslConstants.LanguageName, "Formatting", "Spacing", "126")]

    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension1)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension2)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension3)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension4)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension5)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension6)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension7)]
    [ProvideLanguageExtension(typeof(HlslLanguageInfo), HlslConstants.FileExtension8)]

    [ProvideEditorFactory(typeof(HlslEditorFactory), 140, CommonPhysicalViewAttributes = (int) __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(HlslEditorFactory), VSConstants.LOGVIEWID.TextView_string, IsTrusted = true)]
    [ProvideFileExtensionMapping("{A95B1F48-2A2E-492C-BABE-8DCC8A4643A8}", "HLSL Editor", typeof(HlslEditorFactory), typeof(HlslLanguageInfo), PackageId, 100)]

    [ProvideBraceCompletion(HlslConstants.LanguageName)]

    [ProvideMenuResource("Menus.ctmenu", 1)]

    [ProvideToolWindow(typeof(SyntaxVisualizerToolWindow))]
    internal sealed class HlslPackage : LanguagePackageBase
    {
        // Updated by build process.
        public const string Version = "1.0.0";

        private const string PackageId = "0E01DDB3-F537-4C49-9B50-BDA9DCCE2172";

        public static HlslPackage Instance { get; private set; }

        internal IHlslOptionsService Options { get; private set; }

        protected override CodeWindowManagerBase CreateCodeWindowManager(IVsCodeWindow window)
        {
            return new CodeWindowManager(this, window, this.AsVsServiceProvider());
        }

        protected override LanguageInfoBase CreateLanguageInfo()
        {
            return new HlslLanguageInfo(this);
        }

        protected override EditorFactoryBase CreateEditorFactory()
        {
            return new HlslEditorFactory(this);
        }

        protected override void Initialize()
        {
            SyntaxVisualizerToolWindowCommand.Initialize(this);

            base.Initialize();

            Instance = this;

            Options = this.AsVsServiceProvider().GetComponentModel().GetService<IHlslOptionsService>();
        }
    }
}