using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio;

namespace HlslTools.VisualStudio.Tagging.Classification
{
    [Export]
    internal sealed class ShellEventListener : IVsBroadcastMessageEvents, IDisposable
    {
        public event EventHandler ThemeChanged;

        private readonly IVsShell _shell;
        private readonly uint _broadcastEventCookie;

        [ImportingConstructor]
        public ShellEventListener([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _shell = serviceProvider.GetService<SVsShell, IVsShell>();
            ErrorHandler.ThrowOnFailure(_shell.AdviseBroadcastMessages(this, out _broadcastEventCookie));
        }

        private void RaiseThemeChanged()
        {
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        public int OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            // ReSharper disable once InconsistentNaming
            const uint WM_SYSCOLORCHANGE = 0x0015u;
            if (msg == WM_SYSCOLORCHANGE)
                RaiseThemeChanged();
            return VSConstants.S_OK;
        }

        public void Dispose()
        {
            _shell.UnadviseBroadcastMessages(_broadcastEventCookie);
        }
    }
}