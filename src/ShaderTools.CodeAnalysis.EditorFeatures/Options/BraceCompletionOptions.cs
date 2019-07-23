// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.CodeAnalysis.Editor.Options
{
    internal static class BraceCompletionOptions
    {
        private const string RegistryPath = LocalUserProfileStorageLocation.RootRegistryPath + @"TextEditor\%LANGUAGE%\BraceCompletion\";

        // This is serialized by the Visual Studio-specific LanguageSettingsPersister
        [ExportOption]
        public static readonly PerLanguageOption<bool> EnableBraceCompletion = new PerLanguageOption<bool>(nameof(BraceCompletionOptions), nameof(EnableBraceCompletion), defaultValue: true);

        [ExportOption]
        public static readonly PerLanguageOption<bool> AddSemicolonForTypes = new PerLanguageOption<bool>(nameof(BraceCompletionOptions), nameof(AddSemicolonForTypes), defaultValue: true,
            storageLocations: new OptionStorageLocation[] {
                new LocalUserProfileStorageLocation(RegistryPath + nameof(AddSemicolonForTypes))});
    }
}
