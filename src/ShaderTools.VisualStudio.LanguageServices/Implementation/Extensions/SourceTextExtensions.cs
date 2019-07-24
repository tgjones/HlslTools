// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using VsTextSpan = Microsoft.VisualStudio.TextManager.Interop.TextSpan;

namespace ShaderTools.VisualStudio.LanguageServices.Implementation.Extensions
{
    internal static class SourceTextExtensions
    {
        public static VsTextSpan GetVsTextSpanForSpan(this SourceText text, TextSpan textSpan)
        {
            text.GetLinesAndOffsets(textSpan, out var startLine, out var startOffset, out var endLine, out var endOffset);

            return new VsTextSpan()
            {
                iStartLine = startLine,
                iStartIndex = startOffset,
                iEndLine = endLine,
                iEndIndex = endOffset
            };
        }
    }
}
