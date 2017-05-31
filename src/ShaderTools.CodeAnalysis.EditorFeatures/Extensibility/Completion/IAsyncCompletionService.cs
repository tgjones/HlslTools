// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.Completion;

namespace ShaderTools.CodeAnalysis.Editor
{
    internal interface IAsyncCompletionService
    {
        bool TryGetController(ITextView textView, ITextBuffer subjectBuffer, out Controller controller);
    }
}
