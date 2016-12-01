using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.VisualStudio.Core.Util.Extensions;

namespace ShaderTools.VisualStudio.Core.Navigation
{
    internal abstract class LanguageInfoBase : IVsLanguageInfo
    {
        private readonly LanguagePackageBase _languagePackage;
        private readonly SVsServiceProvider _serviceProvider;

        protected LanguageInfoBase(LanguagePackageBase languagePackage)
        {
            _languagePackage = languagePackage;
            _serviceProvider = languagePackage.AsVsServiceProvider();
        }

        protected abstract string LanguageName { get; }
        protected abstract IEnumerable<string> FileExtensions { get; }

        public int GetLanguageName(out string bstrName)
        {
            bstrName = LanguageName;
            return VSConstants.S_OK;
        }

        public int GetFileExtensions(out string pbstrExtensions)
        {
            pbstrExtensions = string.Join(";", FileExtensions);
            return VSConstants.S_OK;
        }

        public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer)
        {
            ppColorizer = null;
            return VSConstants.E_FAIL;
        }

        public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr)
        {
            var adaptersFactory = _serviceProvider.GetComponentModel().GetService<IVsEditorAdaptersFactoryService>();

            IVsTextLines textLines;
            ErrorHandler.ThrowOnFailure(pCodeWin.GetBuffer(out textLines));
            var textBuffer = adaptersFactory.GetDataBuffer(textLines);
            if (textBuffer == null)
            {
                ppCodeWinMgr = null;
                return VSConstants.E_FAIL;
            }

            ppCodeWinMgr = _languagePackage.GetOrCreateCodeWindowManager(pCodeWin);
            return VSConstants.S_OK;
        }
    }
}