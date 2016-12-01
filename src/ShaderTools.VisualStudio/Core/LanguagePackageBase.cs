using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.VisualStudio.Core.Navigation;

namespace ShaderTools.VisualStudio.Core
{
    internal abstract class LanguagePackageBase : Package
    {
        private readonly Dictionary<IVsCodeWindow, CodeWindowManagerBase> _codeWindowManagers = new Dictionary<IVsCodeWindow, CodeWindowManagerBase>();

        internal CodeWindowManagerBase GetOrCreateCodeWindowManager(IVsCodeWindow window)
        {
            CodeWindowManagerBase value;
            if (!_codeWindowManagers.TryGetValue(window, out value))
                _codeWindowManagers[window] = value = CreateCodeWindowManager(window);
            return value;
        }

        protected abstract CodeWindowManagerBase CreateCodeWindowManager(IVsCodeWindow window);

        internal IEnumerable<CodeWindowManagerBase> CodeWindowManagers => _codeWindowManagers.Values;

        internal LanguagePreferences LanguagePreferences { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();

            // Proffer the service.
            var languageInfo = CreateLanguageInfo();
            ((IServiceContainer)this).AddService(languageInfo.GetType(), languageInfo, true);

            // Hook up language preferences.
            var textMgr = (IVsTextManager) GetService(typeof(SVsTextManager));

            var langPrefs = new LANGPREFERENCES[1];
            langPrefs[0].guidLang = languageInfo.GetType().GUID;
            ErrorHandler.ThrowOnFailure(textMgr.GetUserPreferences(null, null, langPrefs, null));
            LanguagePreferences = new LanguagePreferences(this, langPrefs[0]);

            var textManagerEvents2Guid = typeof(IVsTextManagerEvents2).GUID;
            IConnectionPoint textManagerEvents2ConnectionPoint;
            ((IConnectionPointContainer)textMgr).FindConnectionPoint(ref textManagerEvents2Guid, out textManagerEvents2ConnectionPoint);
            uint cookie;
            textManagerEvents2ConnectionPoint.Advise(LanguagePreferences, out cookie);

            RegisterEditorFactory(CreateEditorFactory());
        }

        protected abstract LanguageInfoBase CreateLanguageInfo();
        protected abstract EditorFactoryBase CreateEditorFactory();

        internal TOptionsPage GetDialogPage<TOptionsPage>()
            where TOptionsPage : DialogPage
        {
            return (TOptionsPage)GetDialogPage(typeof(TOptionsPage));
        }

        internal new object GetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var window in _codeWindowManagers.Values)
                window.RemoveAdornments();
            _codeWindowManagers.Clear();

            base.Dispose(disposing);
        }
    }
}