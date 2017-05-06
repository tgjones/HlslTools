using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.ShaderLab.Formatting;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Options
{
    [Export(typeof(IShaderLabOptionsService))]
    [ExportLanguageService(typeof(IOptionsService), LanguageNames.ShaderLab)]
    internal sealed class OptionsService : IShaderLabOptionsService
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
            AdvancedOptions = GetDialogPage<ShaderLabAdvancedOptionsPage>().Options;

            FormattingOptions = new FormattingOptions
            {
                SpacesPerIndent = ShaderLabPackage.Instance.LanguagePreferences.SpacesPerIndent
            };

            GeneralOptions = GetDialogPage<ShaderLabFormattingGeneralOptionsPage>().Options;

            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }

        public AdvancedOptions AdvancedOptions { get; private set; }
        public FormattingOptions FormattingOptions { get; private set; }
        public GeneralOptions GeneralOptions { get; private set; }

        private static TOptionsPage GetDialogPage<TOptionsPage>()
            where TOptionsPage : DialogPage
        {
            return ShaderLabPackage.Instance.GetDialogPage<TOptionsPage>();
        }
    }
}