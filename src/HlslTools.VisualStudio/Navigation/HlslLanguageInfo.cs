using System.Runtime.InteropServices;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HlslTools.VisualStudio.Navigation
{
    [Guid("80329450-4B0D-4EC7-A4E4-A57C024888D5")]
    internal sealed class HlslLanguageInfo : IVsLanguageInfo
    {
        private readonly SVsServiceProvider _serviceProvider;

        public HlslLanguageInfo(SVsServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public int GetLanguageName(out string bstrName)
        {
            bstrName = HlslConstants.LanguageName;
            return VSConstants.S_OK;
        }

        public int GetFileExtensions(out string pbstrExtensions)
        {
            pbstrExtensions = string.Join(";", HlslConstants.FileExtensions);
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

            ppCodeWinMgr = _serviceProvider.GetHlslToolsService().GetOrCreateCodeWindowManager(pCodeWin);
            return VSConstants.S_OK;
        }
    }
}