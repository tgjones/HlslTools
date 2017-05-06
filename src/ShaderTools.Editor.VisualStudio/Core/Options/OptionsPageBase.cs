using System;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.Editor.VisualStudio.Core.Options.Views;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.Core.Options
{
    internal abstract class OptionsPageBase<TOptionsService, TOptions> : UIElementDialogPage
        where TOptionsService : class, IOptionsService
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

            var optionsService = Site.AsVsServiceProvider().GetComponentModel().GetService<TOptionsService>();
            optionsService.RaiseOptionsChanged();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _control?.Close();
        }
    }
}