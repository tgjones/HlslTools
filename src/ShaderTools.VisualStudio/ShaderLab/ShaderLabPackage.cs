using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.VisualStudio.Core;
using ShaderTools.VisualStudio.Core.Navigation;
using ShaderTools.VisualStudio.Core.Util.Extensions;

namespace ShaderTools.VisualStudio.ShaderLab
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(PackageId)]

    [ProvideLanguageService(typeof(ShaderLabLanguageInfo), ShaderLabConstants.LanguageName, 0,
        ShowCompletion = true,
        ShowSmartIndent = true,
        ShowDropDownOptions = true,
        EnableAdvancedMembersOption = true,
        DefaultToInsertSpaces = true,
        EnableLineNumbers = true,
        RequestStockColors = true)]

    [ProvideLanguageExtension(typeof(ShaderLabLanguageInfo), ShaderLabConstants.FileExtension)]

    [ProvideEditorFactory(typeof(ShaderLabEditorFactory), 150, CommonPhysicalViewAttributes = (int) __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(ShaderLabEditorFactory), VSConstants.LOGVIEWID.TextView_string, IsTrusted = true)]
    [ProvideFileExtensionMapping("{C911385A-6AF2-4C17-B1FE-4D29F6E58B31}", "ShaderLab Editor", typeof(ShaderLabEditorFactory), typeof(ShaderLabLanguageInfo), PackageId, 100)]
    internal sealed class ShaderLabPackage : LanguagePackageBase
    {
        private const string PackageId = "448635DC-AF8B-4767-BFDB-26ED6A865158";

        protected override CodeWindowManagerBase CreateCodeWindowManager(IVsCodeWindow window)
        {
            return new CodeWindowManager(this, window, this.AsVsServiceProvider());
        }

        protected override LanguageInfoBase CreateLanguageInfo()
        {
            return new ShaderLabLanguageInfo(this);
        }

        protected override EditorFactoryBase CreateEditorFactory()
        {
            return new ShaderLabEditorFactory(this);
        }

        private sealed class CodeWindowManager : CodeWindowManagerBase
        {
            public CodeWindowManager(LanguagePackageBase languagePackage, IVsCodeWindow codeWindow, SVsServiceProvider serviceProvider)
                : base(languagePackage, codeWindow, serviceProvider)
            {
            }

            protected override bool TryCreateDropdownBarClient(out int comboBoxCount, out IVsDropdownBarClient client)
            {
                comboBoxCount = 0;
                client = null;
                return false;
            }
        }
    }
}
