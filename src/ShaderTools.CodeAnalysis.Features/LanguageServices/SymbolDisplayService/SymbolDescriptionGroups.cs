// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace ShaderTools.CodeAnalysis.LanguageServices
{
    [Flags]
    internal enum SymbolDescriptionGroups
    {
        None = 0,
        MainDescription = 1 << 0,
        Documentation = 1 << 2,
        All = MainDescription | Documentation
    }
}
