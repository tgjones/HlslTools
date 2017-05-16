namespace ShaderTools.CodeAnalysis.Options
{
    internal static class FeatureOnOffOptions
    {
        private const string RegistryPath = LocalUserProfileStorageLocation.RootRegistryPath + @"TextEditor\%LANGUAGE%\Features\";

        public static readonly PerLanguageOption<bool> AutoFormattingOnSemicolon = new PerLanguageOption<bool>(nameof(FeatureOnOffOptions), nameof(AutoFormattingOnSemicolon), defaultValue: true,
            storageLocations: new OptionStorageLocation[] {
                new LocalUserProfileStorageLocation(RegistryPath + nameof(AutoFormattingOnSemicolon))});

        public static readonly PerLanguageOption<bool> AutoFormattingOnCloseBrace = new PerLanguageOption<bool>(nameof(FeatureOnOffOptions), nameof(AutoFormattingOnCloseBrace), defaultValue: true,
            storageLocations: new OptionStorageLocation[] {
                new LocalUserProfileStorageLocation(RegistryPath + nameof(AutoFormattingOnCloseBrace))});

        public static readonly PerLanguageOption<bool> FormatOnPaste = new PerLanguageOption<bool>(nameof(FeatureOnOffOptions), nameof(FormatOnPaste), defaultValue: true,
            storageLocations: new OptionStorageLocation[] {
                new LocalUserProfileStorageLocation(RegistryPath + nameof(FormatOnPaste))});

        public static readonly PerLanguageOption<bool> IntelliSense = new PerLanguageOption<bool>(nameof(FeatureOnOffOptions), nameof(IntelliSense), defaultValue: true,
            storageLocations: new OptionStorageLocation[] {
                new LocalUserProfileStorageLocation(RegistryPath + nameof(IntelliSense))});

        public static readonly PerLanguageOption<bool> Outlining = new PerLanguageOption<bool>(nameof(FeatureOnOffOptions), nameof(Outlining), defaultValue: true,
            storageLocations: new OptionStorageLocation[] {
                new LocalUserProfileStorageLocation(RegistryPath + nameof(Outlining))});
    }
}
