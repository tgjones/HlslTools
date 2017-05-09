using System;
using System.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Options
{
    [Export(typeof(IHlslOptionsService))]
    [ExportLanguageService(typeof(IOptionsService), LanguageNames.Hlsl)]
    internal sealed class OptionsService : IHlslOptionsService
    {
        public event EventHandler OptionsChanged;

        bool IOptionsService.EnableIntelliSense => AdvancedOptions.EnableIntelliSense;
        bool IOptionsService.EnableErrorReporting => AdvancedOptions.EnableErrorReporting;
        bool IOptionsService.EnableSquiggles => AdvancedOptions.EnableSquiggles;

        public OptionsService()
        {
            RaiseOptionsChanged();
        }

        public void RaiseOptionsChanged()
        {
            AdvancedOptions = GetDialogPage<HlslAdvancedOptionsPage>().Options;

            var indentationOptions = GetDialogPage<HlslFormattingIndentationOptionsPage>().Options;
            var newLinesOptions = GetDialogPage<HlslFormattingNewLinesOptionsPage>().Options;
            var spacingOptions = GetDialogPage<HlslFormattingSpacingOptionsPage>().Options;

            FormattingOptions = new FormattingOptions
            {
                Indentation = indentationOptions,
                NewLines = newLinesOptions,
                Spacing = spacingOptions,

                SpacesPerIndent = HlslPackage.Instance.LanguagePreferences.SpacesPerIndent
            };

            GeneralOptions = GetDialogPage<HlslFormattingGeneralOptionsPage>().Options;

            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }

        public AdvancedOptions AdvancedOptions { get; private set; }
        public FormattingOptions FormattingOptions { get; private set; }
        public GeneralOptions GeneralOptions { get; private set; }

        private static TOptionsPage GetDialogPage<TOptionsPage>()
            where TOptionsPage : DialogPage
        {
            // TODO: Remove this.
            if (HlslPackage.Instance == null)
                ((IVsShell) Package.GetGlobalService(typeof(SVsShell))).LoadPackage<HlslPackage>();

            return HlslPackage.Instance.GetDialogPage<TOptionsPage>();
        }
    }
}