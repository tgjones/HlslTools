using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.CodeAnalysis.Structure
{
    internal static class BlockStructureOptions
    {
        internal const string LocalRegistryPath = @"ShaderTools\Features\BlockStructure\";

        public static readonly PerLanguageOption<bool> EnterOutliningModeWhenFilesOpen = new PerLanguageOption<bool>(
            nameof(BlockStructureOptions), nameof(EnterOutliningModeWhenFilesOpen), defaultValue: true,
            storageLocations: new LocalUserProfileStorageLocation(LocalRegistryPath + nameof(EnterOutliningModeWhenFilesOpen)));
    }
}
