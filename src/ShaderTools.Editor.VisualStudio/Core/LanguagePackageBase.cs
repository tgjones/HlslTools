using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.Editor.VisualStudio.Core.Navigation;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;
using ShaderTools.VisualStudio.LanguageServices;

namespace ShaderTools.Editor.VisualStudio.Core
{
    internal abstract class LanguagePackageBase : Package
    {
        private readonly Dictionary<IVsCodeWindow, CodeWindowManagerBase> _codeWindowManagers = new Dictionary<IVsCodeWindow, CodeWindowManagerBase>();

        internal IComponentModel ComponentModel => (IComponentModel) GetService(typeof(SComponentModel));

        internal CodeWindowManagerBase GetOrCreateCodeWindowManager(IVsCodeWindow window)
        {
            CodeWindowManagerBase value;
            if (!_codeWindowManagers.TryGetValue(window, out value))
                _codeWindowManagers[window] = value = CreateCodeWindowManager(window);
            return value;
        }

        protected abstract CodeWindowManagerBase CreateCodeWindowManager(IVsCodeWindow window);

        internal LanguageInfoBase LanguageInfo { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();

            // Proffer the service.
            LanguageInfo = CreateLanguageInfo();
            ((IServiceContainer)this).AddService(LanguageInfo.GetType(), LanguageInfo, true);

            var shell = (IVsShell) GetService(typeof(SVsShell));
            shell.LoadPackage<ShaderToolsPackage>();
        }

        protected abstract LanguageInfoBase CreateLanguageInfo();

        protected override void Dispose(bool disposing)
        {
            foreach (var window in _codeWindowManagers.Values)
                window.RemoveAdornments();
            _codeWindowManagers.Clear();

            base.Dispose(disposing);
        }
    }
}