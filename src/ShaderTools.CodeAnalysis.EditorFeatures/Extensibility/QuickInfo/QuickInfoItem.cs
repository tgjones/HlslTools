// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Editor
{
    internal class QuickInfoItem
    {
        public TextSpan TextSpan { get; }
        public IDeferredQuickInfoContent Content { get; }

        public QuickInfoItem(TextSpan textSpan, IDeferredQuickInfoContent content)
        {
            this.TextSpan = textSpan;
            this.Content = content;
        }
    }
}
