// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Options;

namespace ShaderTools.CodeAnalysis.Formatting
{
    public static class FormattingOptions
    {
        public static PerLanguageOption<bool> UseTabs { get; } =
            new PerLanguageOption<bool>(nameof(FormattingOptions), nameof(UseTabs), defaultValue: false);

        // This is also serialized by the Visual Studio-specific LanguageSettingsPersister
        public static PerLanguageOption<int> TabSize { get; } =
            new PerLanguageOption<int>(nameof(FormattingOptions), nameof(TabSize), defaultValue: 4);

        // This is also serialized by the Visual Studio-specific LanguageSettingsPersister
        public static PerLanguageOption<int> IndentationSize { get; } =
            new PerLanguageOption<int>(nameof(FormattingOptions), nameof(IndentationSize), defaultValue: 4);

        // This is also serialized by the Visual Studio-specific LanguageSettingsPersister
        public static PerLanguageOption<IndentStyle> SmartIndent { get; } =
            new PerLanguageOption<IndentStyle>(nameof(FormattingOptions), nameof(SmartIndent), defaultValue: IndentStyle.Smart);

        public static PerLanguageOption<string> NewLine { get; } =
            new PerLanguageOption<string>(nameof(FormattingOptions), nameof(NewLine), defaultValue: "\r\n");

        public enum IndentStyle
        {
            None = 0,
            Block = 1,
            Smart = 2
        }
    }
}
