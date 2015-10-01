using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using EnvDTE;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace HlslTools.VisualStudio.Util
{
    // Based on https://github.com/fsprojects/VisualFSharpPowerTools/blob/master/src/FSharpVSPowerTools.Logic/ThemeManager.fs
    [Export]
    internal sealed class ThemeManager
    {
        private readonly static IDictionary<Guid, VisualStudioTheme> Themes = new Dictionary<Guid, VisualStudioTheme>
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
            var themeId = GetThemeId();

            if (themeId == null)
                return VisualStudioTheme.Unknown;

            Guid themeGuid;
            if (!Guid.TryParse(themeId, out themeGuid))
                return VisualStudioTheme.Unknown;

            VisualStudioTheme theme;
            if (!Themes.TryGetValue(themeGuid, out theme))
                return VisualStudioTheme.Unknown;

            return theme;
        }

        private string GetThemeId()
        {
            var dte = _serviceProvider.GetService<SDTE, DTE>();
            var version = VisualStudioVersionUtility.FromDteVersion(dte.Version);

            string registryKeyName, themePropertyName;
            switch (version)
            {
                case VisualStudioVersion.Vs2012:
                    registryKeyName = @"Software\Microsoft\VisualStudio\11.0\General";
                    themePropertyName = "CurrentTheme";
                    break;
                case VisualStudioVersion.Vs2013:
                    registryKeyName = @"Software\Microsoft\VisualStudio\12.0\General";
                    themePropertyName = "CurrentTheme";
                    break;
                case VisualStudioVersion.Vs2015:
                    registryKeyName = @"Software\Microsoft\VisualStudio\14.0\ApplicationPrivateSettings\Microsoft\VisualStudio";
                    themePropertyName = "ColorTheme";
                    break;
                default:
                    return null;
            }

            using (var key = Registry.CurrentUser.OpenSubKey(registryKeyName))
            {
                var keyValue = key?.GetValue(themePropertyName, null) as string;
                if (keyValue == null)
                    return null;

                switch (version)
                {
                    case VisualStudioVersion.Vs2012:
                    case VisualStudioVersion.Vs2013:
                        return keyValue;
                    case VisualStudioVersion.Vs2015:
                        // 0*System.String*1ded0138-47ce-435e-84ef-9ec1f439b749
                        var splitValue = keyValue.Split('*');
                        return (splitValue.Length == 3)
                            ? splitValue[2]
                            : null;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}