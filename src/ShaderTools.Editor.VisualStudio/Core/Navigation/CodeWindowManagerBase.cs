using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.CodeAnalysis.Editor.Options;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.VisualStudio.LanguageServices;

namespace ShaderTools.Editor.VisualStudio.Core.Navigation
{
    internal abstract class CodeWindowManagerBase : IVsCodeWindowManager
    {
        private readonly LanguagePackageBase _languagePackage;
        private readonly IOptionService _optionService;
        private IVsDropdownBarClient _dropdownBarClient;

        protected IComponentModel ComponentModel => _languagePackage.ComponentModel;

        protected CodeWindowManagerBase(LanguagePackageBase languagePackage, IVsCodeWindow codeWindow, SVsServiceProvider serviceProvider)
        {
            _languagePackage = languagePackage;
            CodeWindow = codeWindow;
            ServiceProvider = serviceProvider;

            var workspace = languagePackage.ComponentModel.GetService<VisualStudioWorkspace>();
            _optionService = workspace.Services.GetService<IOptionService>();

            _optionService.OptionChanged += OnOptionChanged;
        }

        private void OnOptionChanged(object sender, OptionChangedEventArgs e)
        {
            if (e.Language != _languagePackage.LanguageInfo.LanguageName ||
                e.Option != NavigationBarOptions.ShowNavigationBar)
            {
                return;
            }

            var enabled = _optionService.GetOption(NavigationBarOptions.ShowNavigationBar, _languagePackage.LanguageInfo.LanguageName);
            if (enabled)
                AddDropDownBar();
            else
                RemoveDropDownBar();
        }

        private void SetupView(IVsTextView view)
        {
            _languagePackage.LanguageInfo.SetupNewTextView(view);
        }

        public IVsCodeWindow CodeWindow { get; }
        public SVsServiceProvider ServiceProvider { get; }

        public int AddAdornments()
        {
            IVsTextView textView;
            if (ErrorHandler.Succeeded(CodeWindow.GetPrimaryView(out textView)) && textView != null)
                ErrorHandler.ThrowOnFailure(OnNewView(textView));
            if (ErrorHandler.Succeeded(CodeWindow.GetSecondaryView(out textView)) && textView != null)
                ErrorHandler.ThrowOnFailure(OnNewView(textView));

            var enabled = _optionService.GetOption(NavigationBarOptions.ShowNavigationBar, _languagePackage.LanguageInfo.LanguageName);

            if (enabled)
                return AddDropDownBar();

            return VSConstants.S_OK;
        }

        private int AddDropDownBar()
        {
            int comboBoxCount;
            IVsDropdownBarClient client;
            if (TryCreateDropdownBarClient(out comboBoxCount, out client))
            {
                ErrorHandler.ThrowOnFailure(AddDropdownBar(comboBoxCount, client));
                _dropdownBarClient = client;
            }

            return VSConstants.S_OK;
        }

        public int OnNewView(IVsTextView view)
        {
            SetupView(view);

            return VSConstants.S_OK;
        }

        int IVsCodeWindowManager.OnNewView(IVsTextView pView)
        {
            if (pView == null)
                throw new ArgumentNullException(nameof(pView));
            return OnNewView(pView);
        }

        public int RemoveAdornments()
        {
            var result = RemoveDropDownBar();

            _optionService.OptionChanged -= OnOptionChanged;

            return result;
        }

        private int RemoveDropDownBar()
        {
            var manager = CodeWindow as IVsDropdownBarManager;
            if (manager == null)
                return VSConstants.E_FAIL;

            IVsDropdownBar dropdownBar;
            int hr = manager.GetDropdownBar(out dropdownBar);
            if (ErrorHandler.Succeeded(hr) && dropdownBar != null)
            {
                IVsDropdownBarClient client;
                hr = dropdownBar.GetClient(out client);
                if (ErrorHandler.Succeeded(hr) && client == _dropdownBarClient)
                {
                    _dropdownBarClient = null;
                    return manager.RemoveDropdownBar();
                }
            }

            _dropdownBarClient = null;

            return VSConstants.S_OK;
        }

        protected abstract bool TryCreateDropdownBarClient(out int comboBoxCount, out IVsDropdownBarClient client);

        private int AddDropdownBar(int comboBoxCount, IVsDropdownBarClient client)
        {
            var manager = CodeWindow as IVsDropdownBarManager;
            if (manager == null)
                throw new NotSupportedException();

            IVsDropdownBar dropdownBar;
            var hr = manager.GetDropdownBar(out dropdownBar);
            if (ErrorHandler.Succeeded(hr) && dropdownBar != null)
            {
                hr = manager.RemoveDropdownBar();
                if (ErrorHandler.Failed(hr))
                    return hr;
            }

            return manager.AddDropdownBar(comboBoxCount, client);
        }
    }
}