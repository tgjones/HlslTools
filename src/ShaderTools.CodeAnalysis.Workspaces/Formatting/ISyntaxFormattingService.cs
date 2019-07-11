// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Formatting
{
    internal interface ISyntaxFormattingService : ILanguageService
    {
        string Format(SyntaxTreeBase tree, TextSpan span, OptionSet options, CancellationToken cancellationToken);

        Task<IFormattingResult> FormatAsync(SyntaxTreeBase tree, SyntaxNodeBase node, IEnumerable<TextSpan> spans, OptionSet options, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Contains changes that can be either applied to different targets such as a buffer or a tree
    /// or examined to be used in other places such as quick fix.
    /// </summary>
    internal interface IFormattingResult
    {
        IList<TextChange> GetTextChanges(CancellationToken cancellationToken = default(CancellationToken));
    }

    internal sealed class FormattingResult : IFormattingResult
    {
        private readonly IList<TextChange> _changes;

        public FormattingResult(IList<TextChange> changes)
        {
            _changes = changes;
        }

        public IList<TextChange> GetTextChanges(CancellationToken cancellationToken)
        {
            return _changes;
        }
    }
}
