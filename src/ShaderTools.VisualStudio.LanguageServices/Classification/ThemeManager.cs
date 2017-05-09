using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace ShaderTools.VisualStudio.LanguageServices.Classification
{
    [Export]
    internal sealed class ThemeManager
    {
        private static readonly IDictionary<Guid, VisualStudioTheme> Themes = new Dictionary<Guid, VisualStudioTheme>
        {
            { KnownColorThemes.Light, VisualStudioTheme.Light },
            { KnownColorThemes.Blue, VisualStudioTheme.Blue },
            { KnownColorThemes.Dark, VisualStudioTheme.Dark }
        };

        private readonly IServiceProvider _serviceProvider;

        [ImportingConstructor]
        public ThemeManager([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Based on https://msdn.microsoft.com/en-us/library/microsoft.internal.visualstudio.shell.interop.svscolorthemeservice(v=vs.110).aspx
        [Guid("0d915b59-2ed7-472a-9de8-9161737ea1c5")]
        private interface SVsColorThemeService
        {
        }

        public VisualStudioTheme GetCurrentTheme()
        {
            var themeGuid = GetThemeId();

            VisualStudioTheme theme;
            if (Themes.TryGetValue(themeGuid, out theme))
                return theme;

            try
            {
                var color = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
                return color.GetBrightness() > 0.5f
                    ? VisualStudioTheme.Dark
                    : VisualStudioTheme.Light;
            }
            catch
            {
                //Logger.Log("Can't read Visual Studio themes from environment colors.");
                return VisualStudioTheme.Unknown;
            }
        }

        private Guid GetThemeId()
        {
            try
            {
                dynamic themeService = _serviceProvider.GetService(typeof(SVsColorThemeService));
                return (Guid) themeService.CurrentTheme.ThemeId;
            }
            catch
            {
                return Guid.Empty;
            }
        }
    }
}
