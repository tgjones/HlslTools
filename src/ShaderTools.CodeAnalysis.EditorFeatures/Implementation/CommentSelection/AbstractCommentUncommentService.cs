// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Formatting;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.CommentSelection
{
    internal abstract class AbstractCommentUncommentService : ICommentUncommentService
    {
        public abstract string BlockCommentEndString { get; }
        public abstract string BlockCommentStartString { get; }
        public abstract string SingleLineCommentString { get; }
        public abstract bool SupportsBlockComment { get; }

        public Document Format(Document document, IEnumerable<TextSpan> changes, CancellationToken cancellationToken)
        {
            //var root = document.GetSyntaxRootSynchronously(cancellationToken);
            //var formattingSpans = changes.Select(s => CommonFormattingHelpers.GetFormattingSpan(root, s));

            return Formatter.FormatAsync(document, changes, cancellationToken: cancellationToken).WaitAndGetResult(cancellationToken);
        }
    }
}