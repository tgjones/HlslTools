using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.Editor.VisualStudio.Core;
using ShaderTools.Editor.VisualStudio.Core.Navigation;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;
using ShaderTools.VisualStudio.LanguageServices;

namespace ShaderTools.Editor.VisualStudio.ShaderLab
{
    //[PackageRegistration(UseManagedResourcesOnly = true)]
    //[Guid(Guids.ShaderLabPackageIdString)]

    //[ProvideLanguageService(typeof(ShaderLabLanguageInfo), ShaderLabConstants.LanguageName, 0,
    //    ShowCompletion = true,
    //    ShowSmartIndent = true,
    //    ShowDropDownOptions = true,
    //    EnableAdvancedMembersOption = true,
    //    DefaultToInsertSpaces = true,
    //    EnableLineNumbers = true,
    //    RequestStockColors = true)]

    //[ProvideLanguageEditorOptionPage(typeof(ShaderLabAdvancedOptionsPage), ShaderLabConstants.LanguageName, null, "Advanced", "120")]
    //[ProvideLanguageEditorOptionPage(typeof(ShaderLabFormattingGeneralOptionsPage), ShaderLabConstants.LanguageName, null, "Formatting", "122")]

    //[ProvideLanguageExtension(typeof(ShaderLabLanguageInfo), ".shader")]

    //[ProvideFileExtensionMapping("{C911385A-6AF2-4C17-B1FE-4D29F6E58B31}", "ShaderLab Editor", typeof(ShaderLabEditorFactory), typeof(ShaderLabLanguageInfo), PackageId, 100)]
    internal sealed class ShaderLabPackage : LanguagePackageBase
    {
        protected override CodeWindowManagerBase CreateCodeWindowManager(IVsCodeWindow window)
        {
            return new CodeWindowManager(this, window, this.AsVsServiceProvider());
        }

        protected override LanguageInfoBase CreateLanguageInfo()
        {
            return new ShaderLabLanguageInfo(this);
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
