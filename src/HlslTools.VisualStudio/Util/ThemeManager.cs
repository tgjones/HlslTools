using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace HlslTools.VisualStudio.Util
{
    [Guid("0d915b59-2ed7-472a-9de8-9161737ea1c5")]
    internal interface SVsColorThemeService
    {
    }

    // Based on https://github.com/fsprojects/VisualFSharpPowerTools/blob/master/src/FSharpVSPowerTools.Logic/ThemeManager.fs
    [Export]
    internal sealed class ThemeManager
    {
        private static readonly IDictionary<Guid, VisualStudioTheme> Themes = new Dictionary<Guid, VisualStudioTheme>
        {
            { new Guid("de3dbbcd-f642-433c-8353-8f1df4370aba"), VisualStudioTheme.Light },
            { new Guid("a4d6a176-b948-4b29-8c66-53c97a1ed7d0"), VisualStudioTheme.Blue },
            { new Guid("1ded0138-47ce-435e-84ef-9ec1f439b749"), VisualStudioTheme.Dark }
        };

        private readonly IServiceProvider _serviceProvider;

        [ImportingConstructor]
        public ThemeManager([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
                Logger.Log("Can't read Visual Studio themes from environment colors.");
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