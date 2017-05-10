using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.CodeAnalysis.Diagnostics
{
    internal static class DiagnosticsOptions
    {
        internal const string LocalRegistryPath = @"ShaderTools\Features\Diagnostics\";

        public static readonly PerLanguageOption<bool> EnableErrorReporting = new PerLanguageOption<bool>(
            nameof(DiagnosticsOptions), nameof(EnableErrorReporting), defaultValue: true,
            storageLocations: new LocalUserProfileStorageLocation(LocalRegistryPath + nameof(EnableErrorReporting)));

        public static readonly PerLanguageOption<bool> EnableSquiggles = new PerLanguageOption<bool>(
            nameof(DiagnosticsOptions), nameof(EnableSquiggles), defaultValue: true,
            storageLocations: new LocalUserProfileStorageLocation(LocalRegistryPath + nameof(EnableSquiggles)));
    }
}
