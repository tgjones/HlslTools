using System;
using System.Windows;
using HlslTools.VisualStudio.Options.Views;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Shell;

namespace HlslTools.VisualStudio.Options
{
    internal abstract class OptionsPageBase<TOptions> : UIElementDialogPage
        where TOptions : class, new()
    {
        private OptionsControlBase _control;
        private TOptions _options;

        protected override UIElement Child => _control ?? (_control = CreateControl(Site));

        protected abstract OptionsControlBase CreateControl(IServiceProvider serviceProvider);

        public override object AutomationObject => Options;

        public TOptions Options => _options ?? (_options = new TOptions());

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            var optionsService = Site.AsVsServiceProvider().GetComponentModel().GetService<IOptionsService>();
            optionsService.RaiseOptionsChanged();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _control?.Close();
        }
    }
}