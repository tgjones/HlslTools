using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Editor;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;
using ShaderTools.Utilities.Diagnostics;
using ShaderTools.VisualStudio.LanguageServices;
using ShaderTools.VisualStudio.LanguageServices.Implementation;

namespace ShaderTools.Editor.VisualStudio.Core
{
    internal abstract class LanguageInfoBase : IVsLanguageInfo
    {
        private readonly LanguagePackageBase _languagePackage;
        private readonly SVsServiceProvider _serviceProvider;

        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactory;

        protected LanguageInfoBase(LanguagePackageBase languagePackage)
        {
            _languagePackage = languagePackage;
            _serviceProvider = languagePackage.AsVsServiceProvider();

            _editorAdaptersFactory = languagePackage.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
        }

        internal abstract string LanguageName { get; }

        public int GetLanguageName(out string bstrName)
        {
            bstrName = LanguageName;
            return VSConstants.S_OK;
        }

        public int GetFileExtensions(out string pbstrExtensions)
        {
            pbstrExtensions = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer)
        {
            ppColorizer = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr)
        {
            IVsTextLines textLines;
            ErrorHandler.ThrowOnFailure(pCodeWin.GetBuffer(out textLines));
            var textBuffer = _editorAdaptersFactory.GetDataBuffer(textLines);
            if (textBuffer == null)
            {
                ppCodeWinMgr = null;
                return VSConstants.E_FAIL;
            }

            ppCodeWinMgr = _languagePackage.GetOrCreateCodeWindowManager(pCodeWin);
            return VSConstants.S_OK;
        }

        internal virtual void SetupNewTextView(IVsTextView textView)
        {
            Contract.ThrowIfNull(textView);

            var wpfTextView = _editorAdaptersFactory.GetWpfTextView(textView);
            Contract.ThrowIfNull(wpfTextView, "Could not get IWpfTextView for IVsTextView");

            //Contract.Assert(!wpfTextView.Properties.ContainsProperty(typeof(AbstractVsTextViewFilter<TPackage, TLanguageService>)));

            var commandHandlerFactory = _languagePackage.ComponentModel.GetService<ICommandHandlerServiceFactory>();
            var workspace = _languagePackage.ComponentModel.GetService<VisualStudioWorkspace>();

            // The lifetime of CommandFilter is married to the view
            wpfTextView.GetOrCreateAutoClosingProperty(v =>
                new StandaloneCommandFilter(_serviceProvider, v, commandHandlerFactory, _editorAdaptersFactory).AttachToVsTextView());

            ConditionallyCollapseOutliningRegions(wpfTextView, workspace);
        }

        private void ConditionallyCollapseOutliningRegions(IWpfTextView wpfTextView, Workspace workspace)
        {
            var outliningManagerService = _languagePackage.ComponentModel.GetService<IOutliningManagerService>();
            var outliningManager = outliningManagerService.GetOutliningManager(wpfTextView);
            if (outliningManager == null)
            {
                return;
            }

            if (!workspace.Options.GetOption(FeatureOnOffOptions.Outlining, this.LanguageName))
            {
                outliningManager.Enabled = false;
            }
        }
    }
}