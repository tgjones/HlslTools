using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.VisualStudio.Hlsl.Editing.BraceCompletion;
using ShaderTools.VisualStudio.Hlsl.Navigation;
using ShaderTools.VisualStudio.Hlsl.Options;
using ShaderTools.VisualStudio.Hlsl.SyntaxVisualizer;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Version, IconResourceID = 400)]
    [Guid(GuidList.guidShaderStudio_VisualStudioPackagePkgString)]
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

    [ProvideEditorFactory(typeof(HlslEditorFactory), 140, CommonPhysicalViewAttributes = (int) __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(HlslEditorFactory), VSConstants.LOGVIEWID.TextView_string, IsTrusted = true)]
    [ProvideFileExtensionMapping("{A95B1F48-2A2E-492C-BABE-8DCC8A4643A8}", "HLSL Editor", typeof(HlslEditorFactory), typeof(HlslLanguageInfo), GuidList.guidShaderStudio_VisualStudioPackagePkgString, 100)]

    [ProvideBraceCompletion(HlslConstants.LanguageName)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(SyntaxVisualizerToolWindow))]
    public sealed class HlslToolsPackage : Package
    {
        // Updated by build process.
        public const string Version = "1.0.0";

        public static HlslToolsPackage Instance { get; private set; }

        internal IOptionsService Options { get; private set; }

        #region Language preferences

        private readonly Dictionary<IVsCodeWindow, CodeWindowManager> _codeWindowManagers = new Dictionary<IVsCodeWindow, CodeWindowManager>();

        internal IEnumerable<CodeWindowManager> CodeWindowManagers => _codeWindowManagers.Values;

        internal HlslLanguagePreferences LanguagePreferences { get; private set; }

        internal CodeWindowManager GetOrCreateCodeWindowManager(IVsCodeWindow window)
        {
            CodeWindowManager value;
            if (!_codeWindowManagers.TryGetValue(window, out value))
                _codeWindowManagers[window] = value = new CodeWindowManager(window, this.AsVsServiceProvider());
            return value;
        }

        #endregion

        protected override void Initialize()
        {
            SyntaxVisualizerToolWindowCommand.Initialize(this);
            base.Initialize();

            Instance = this;

            // Proffer the service.
            var languageInfo = new HlslLanguageInfo(this.AsVsServiceProvider());
            ((IServiceContainer) this).AddService(typeof(HlslLanguageInfo), languageInfo, true);

            // Hook up language preferences.
            var textMgr = (IVsTextManager) GetService(typeof(SVsTextManager));

            var langPrefs = new LANGPREFERENCES[1];
            langPrefs[0].guidLang = typeof(HlslLanguageInfo).GUID;
            ErrorHandler.ThrowOnFailure(textMgr.GetUserPreferences(null, null, langPrefs, null));
            LanguagePreferences = new HlslLanguagePreferences(this, langPrefs[0]);

            var textManagerEvents2Guid = typeof(IVsTextManagerEvents2).GUID;
            IConnectionPoint textManagerEvents2ConnectionPoint;
            ((IConnectionPointContainer)textMgr).FindConnectionPoint(ref textManagerEvents2Guid, out textManagerEvents2ConnectionPoint);
            uint cookie;
            textManagerEvents2ConnectionPoint.Advise(LanguagePreferences, out cookie);

            RegisterEditorFactory(new HlslEditorFactory(this));

            Options = this.AsVsServiceProvider().GetComponentModel().GetService<IOptionsService>();
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var window in _codeWindowManagers.Values)
                window.RemoveAdornments();
            _codeWindowManagers.Clear();

            base.Dispose(disposing);
        }

        internal TOptionsPage GetDialogPage<TOptionsPage>()
            where TOptionsPage : DialogPage
        {
            return (TOptionsPage) GetDialogPage(typeof(TOptionsPage));
        }

        internal new object GetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }
    }
}