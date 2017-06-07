// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace ShaderTools.VisualStudio.LanguageServices.LanguageService
{
    internal abstract partial class AbstractLanguageService
    {
        public abstract Guid LanguageServiceId { get; }
        public abstract IServiceProvider SystemServiceProvider { get; }
    }
}
