using System;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using ShaderTools.VisualStudio.Hlsl.Options.Views;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Options
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