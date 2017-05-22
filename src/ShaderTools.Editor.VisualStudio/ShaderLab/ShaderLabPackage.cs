using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.CodeAnalysis;
using ShaderTools.Editor.VisualStudio.Core;
using ShaderTools.Editor.VisualStudio.Core.Navigation;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;
using ShaderTools.VisualStudio.LanguageServices;
using ShaderTools.VisualStudio.LanguageServices.Registration;
using ShaderTools.CodeAnalysis.Editor.ShaderLab.Projection;

namespace ShaderTools.Editor.VisualStudio.ShaderLab
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.ShaderLabPackageIdString)]

    //[ProvideLanguageService(typeof(ShaderLabLanguageInfo), LanguageNames.ShaderLab, 0,
    //    ShowCompletion = true,
    //    ShowSmartIndent = true,
    //    ShowDropDownOptions = true,
    //    EnableAdvancedMembersOption = true,
    //    DefaultToInsertSpaces = true,
    //    EnableLineNumbers = true,
    //    RequestStockColors = true)]

    //[ProvideService(typeof(ShaderLabLanguageInfo), ServiceName = "ShaderLab Language Service")]

    //[ProvideLanguageEditorOptionPage(typeof(ShaderLabAdvancedOptionsPage), ShaderLabConstants.LanguageName, null, "Advanced", "120")]
    //[ProvideLanguageEditorOptionPage(typeof(ShaderLabFormattingGeneralOptionsPage), ShaderLabConstants.LanguageName, null, "Formatting", "122")]

    //[ProvideLanguageExtension(typeof(ShaderLabLanguageInfo), ".shader")]

    //[ProvideFileExtensionMapping(
    //    "{C911385A-6AF2-4C17-B1FE-4D29F6E58B31}", 
    //    "ShaderLab Editor", 
    //    typeof(IVsEditorFactory), 
    //    typeof(ShaderLabLanguageInfo), 
    //    Guids.ShaderLabPackageIdString, 
    //    100)]

    //[ProvideBraceCompletion(LanguageNames.ShaderLab)]
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

        protected override void Initialize()
        {
            base.Initialize();

            var workspace = ComponentModel.GetService<VisualStudioWorkspace>();
            workspace.Services.GetService<IProjectionBufferService>();
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
