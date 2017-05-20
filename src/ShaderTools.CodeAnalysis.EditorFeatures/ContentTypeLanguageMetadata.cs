// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Editor
{
    internal class ContentTypeLanguageMetadata : LanguageMetadata
    {
        public string DefaultContentType { get; }

        public ContentTypeLanguageMetadata(IDictionary<string, object> data)
            : base(data)
        {
            this.DefaultContentType = (string)data.GetValueOrDefault("DefaultContentType");
        }

        public ContentTypeLanguageMetadata(string defaultContentType, string language)
            : base(language)
        {
            this.DefaultContentType = defaultContentType;
        }
    }
}
