using System;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;

namespace ShaderTools.VisualStudio.Hlsl.Util
{
    // Based on https://github.com/madskristensen/WebEssentials2015/blob/master/EditorExtensions/Shared/Helpers/Logger.cs
    internal static class Logger
    {
        private static readonly Guid HlslToolsOutputWindowGuid = Guid.Parse("E3B8CF6D-2458-4246-9276-C27BA51D10C1");
        private static IVsOutputWindowPane _pane;
        private static readonly object SyncRoot = new object();

        public static void Log(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

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

        public static void Log(Exception ex)
        {
            if (ex != null)
                Log(ex.ToString());
        }

        public static void ShowMessage(string message, string title = "HLSL Tools",
            MessageBoxButtons messageBoxButtons = MessageBoxButtons.OK,
            MessageBoxIcon messageBoxIcon = MessageBoxIcon.Warning,
            MessageBoxDefaultButton messageBoxDefaultButton = MessageBoxDefaultButton.Button1)
        {
            MessageBox.Show(message, title, messageBoxButtons, messageBoxIcon, messageBoxDefaultButton);
        }

        private static bool EnsurePane()
        {
            if (_pane == null)
                lock (SyncRoot)
                    if (_pane == null)
                        _pane = HlslToolsPackage.Instance.GetOutputPane(HlslToolsOutputWindowGuid, "HLSL Tools");

            return _pane != null;
        }
    }
}