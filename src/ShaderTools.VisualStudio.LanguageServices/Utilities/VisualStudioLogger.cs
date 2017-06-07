using System;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ShaderTools.CodeAnalysis.Log;

namespace ShaderTools.VisualStudio.LanguageServices.Utilities
{
    internal sealed class VisualStudioLogger : ILogger
    {
        private static readonly Guid ShaderToolsOutputWindowGuid = Guid.Parse("E3B8CF6D-2458-4246-9276-C27BA51D10C1");

        private readonly Package _package;
        private IVsOutputWindowPane _pane;

        public VisualStudioLogger(Package package)
        {
            _package = package;
        }

        public void Log(string message)
        {
            try
            {
                if (EnsurePane())
                    _pane.OutputString(DateTime.Now + ": " + message + Environment.NewLine);
            }
            catch
            {
                // Do nothing
            }
        }

        private bool EnsurePane()
        {
            if (_pane == null)
                _pane = Interlocked.Exchange(ref _pane, _package.GetOutputPane(ShaderToolsOutputWindowGuid, "HLSL Tools"));

            return _pane != null;
        }
    }
}
