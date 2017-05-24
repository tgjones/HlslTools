// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;
using ShaderTools.CodeAnalysis.Host;

namespace ShaderTools.CodeAnalysis.Text
{
    internal interface ITextBufferCloneService : IWorkspaceService
    {
        ITextBuffer Clone(SnapshotSpan span);
    }
}
