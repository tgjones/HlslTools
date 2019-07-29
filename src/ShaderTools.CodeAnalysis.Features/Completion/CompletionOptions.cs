// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Options;

namespace ShaderTools.CodeAnalysis.Completion
{
    internal static class CompletionOptions
    {
        // This is serialized by the Visual Studio-specific LanguageSettingsPersister
        public static readonly PerLanguageOption<bool> HideAdvancedMembers = new PerLanguageOption<bool>(nameof(CompletionOptions), nameof(HideAdvancedMembers), defaultValue: false);

        // This is serialized by the Visual Studio-specific LanguageSettingsPersister
        public static readonly PerLanguageOption<bool> TriggerOnTyping = new PerLanguageOption<bool>(nameof(CompletionOptions), nameof(TriggerOnTyping), defaultValue: true);

        public static readonly PerLanguageOption<bool> TriggerOnTypingLetters = new PerLanguageOption<bool>(nameof(CompletionOptions), nameof(TriggerOnTypingLetters), defaultValue: true);

        public static readonly PerLanguageOption<bool?> TriggerOnDeletion = new PerLanguageOption<bool?>(nameof(CompletionOptions), nameof(TriggerOnDeletion), defaultValue: null);

        public static readonly PerLanguageOption<EnterKeyRule> EnterKeyBehavior =
            new PerLanguageOption<EnterKeyRule>(nameof(CompletionOptions), nameof(EnterKeyBehavior), defaultValue: EnterKeyRule.Default);
    }
}
