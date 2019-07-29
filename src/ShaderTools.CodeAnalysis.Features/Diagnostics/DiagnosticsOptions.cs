using Microsoft.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.CodeAnalysis.Diagnostics
{
    internal static class DiagnosticsOptions
    {
        private const string RegistryPath = LocalUserProfileStorageLocation.RootRegistryPath + @"TextEditor\%LANGUAGE%\Diagnostics\";

        public static readonly PerLanguageOption<bool> EnableErrorReporting = new PerLanguageOption<bool>(
            nameof(DiagnosticsOptions), nameof(EnableErrorReporting), defaultValue: true,
            storageLocations: new LocalUserProfileStorageLocation(RegistryPath + nameof(EnableErrorReporting)));

        public static readonly PerLanguageOption<bool> EnableSquiggles = new PerLanguageOption<bool>(
            nameof(DiagnosticsOptions), nameof(EnableSquiggles), defaultValue: true,
            storageLocations: new LocalUserProfileStorageLocation(RegistryPath + nameof(EnableSquiggles)));
    }
}
