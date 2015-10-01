using System;
using System.ComponentModel.Composition;
using HlslTools.Formatting;
using Microsoft.VisualStudio.Shell;

namespace HlslTools.VisualStudio.Options
{
    [Export(typeof(IOptionsService))]
    internal sealed class OptionsService : IOptionsService
    {
        public event EventHandler OptionsChanged;

        public void RaiseOptionsChanged()
        {
            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }

        public AdvancedOptions AdvancedOptions => GetDialogPage<HlslAdvancedOptionsPage>().Options;

        public FormattingOptions FormattingOptions
        {
            get
            {
                var indentationOptions = GetDialogPage<HlslFormattingIndentationOptionsPage>().Options;
                var newLinesOptions = GetDialogPage<HlslFormattingNewLinesOptionsPage>().Options;
                var spacingOptions = GetDialogPage<HlslFormattingSpacingOptionsPage>().Options;

                return new FormattingOptions
                {
                    Indentation = indentationOptions,
                    NewLines = newLinesOptions,
                    Spacing = spacingOptions,

                    SpacesPerIndent = HlslToolsPackage.Instance.LanguagePreferences.SpacesPerIndent
                };
            }
        }

        public GeneralOptions GeneralOptions => GetDialogPage<HlslFormattingGeneralOptionsPage>().Options;

        private TOptionsPage GetDialogPage<TOptionsPage>()
            where TOptionsPage : DialogPage
        {
            return HlslToolsPackage.Instance.GetDialogPage<TOptionsPage>();
        }
    }
}