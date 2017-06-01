// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace ShaderTools.CodeAnalysis
{
    internal enum Glyph
    {
        None,

        Assembly,

        HlslFile,

        ClassPublic,
        ClassProtected,
        ClassPrivate,
        ClassInternal,

        ShaderLabFile,

        ConstantPublic,
        ConstantProtected,
        ConstantPrivate,
        ConstantInternal,

        DelegatePublic,
        DelegateProtected,
        DelegatePrivate,
        DelegateInternal,

        EnumPublic,
        EnumProtected,
        EnumPrivate,
        EnumInternal,

        EnumMemberPublic,
        EnumMemberProtected,
        EnumMemberPrivate,
        EnumMemberInternal,

        Error,
        StatusInformation,

        EventPublic,
        EventProtected,
        EventPrivate,
        EventInternal,

        ExtensionMethodPublic,
        ExtensionMethodProtected,
        ExtensionMethodPrivate,
        ExtensionMethodInternal,

        FieldPublic,
        FieldProtected,
        FieldPrivate,
        FieldInternal,

        InterfacePublic,
        InterfaceProtected,
        InterfacePrivate,
        InterfaceInternal,

        Intrinsic,

        Keyword,

        Label,

        Local,

        Macro,

        Namespace,

        MethodPublic,
        MethodProtected,
        MethodPrivate,
        MethodInternal,

        ModulePublic,
        ModuleProtected,
        ModulePrivate,
        ModuleInternal,

        OpenFolder,

        Operator,

        Parameter,

        PropertyPublic,
        PropertyProtected,
        PropertyPrivate,
        PropertyInternal,

        RangeVariable,

        Reference,

        StructurePublic,
        StructureProtected,
        StructurePrivate,
        StructureInternal,

        Typedef,

        TypeParameter,

        Snippet,

        CompletionWarning,

        AddReference,
        NuGet
    }
}
