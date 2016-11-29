using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using ShaderTools.Hlsl.Formatting;

namespace ShaderTools.VisualStudio.Hlsl.Options
{
    [Export(typeof(IOptionsService))]
    internal sealed class OptionsService : IOptionsService
    {
        public event EventHandler OptionsChanged;

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

                SpacesPerIndent = HlslToolsPackage.Instance.LanguagePreferences.SpacesPerIndent
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
            return HlslToolsPackage.Instance.GetDialogPage<TOptionsPage>();
        }
    }
}