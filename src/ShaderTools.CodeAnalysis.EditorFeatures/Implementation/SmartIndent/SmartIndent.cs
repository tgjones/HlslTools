// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Formatting;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.SmartIndent
{
    internal partial class SmartIndent : ISmartIndent
    {
        private readonly ITextView _textView;

        public SmartIndent(ITextView textView)
        {
            _textView = textView ?? throw new ArgumentNullException(nameof(textView));
        }

        public int? GetDesiredIndentation(ITextSnapshotLine line)
        {
            return GetDesiredIndentation(line, CancellationToken.None);
        }

        public void Dispose()
        {
        }

        private int? GetDesiredIndentation(ITextSnapshotLine line, CancellationToken cancellationToken)
        {
            var document = line.Snapshot.AsText().GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
                return null;

            var documentOptions = document.GetOptionsAsync(cancellationToken).WaitAndGetResult(cancellationToken);

            switch (documentOptions.GetOption(FormattingOptions.SmartIndent))
            {
                case FormattingOptions.IndentStyle.None:
                    return null;

                case FormattingOptions.IndentStyle.Block:
                    return DoBlockIndent(line, documentOptions);

                case FormattingOptions.IndentStyle.Smart:
                    return DoSmartIndent(line, document, documentOptions, cancellationToken);

                default:
                    return null;
            }
        }

        private int? DoSmartIndent(ITextSnapshotLine line, Document document, DocumentOptionSet optionSet, CancellationToken cancellationToken)
        {
            var indentationService = document.GetLanguageService<IIndentationService>();
            var syntaxFactsService = document.GetLanguageService<ISyntaxFactsService>();

            var syntaxTree = document.GetSyntaxTreeSynchronously(cancellationToken);

            var indent = FindTotalParentChainIndent(
                syntaxTree.Root,
                line.Start.Position, 
                0,
                optionSet.GetOption(FormattingOptions.IndentationSize),
                indentationService,
                syntaxFactsService);

            return indent;
        }

        // From https://github.com/KirillOsenkov/XmlParser/blob/master/src/Microsoft.Language.Xml.Editor/SmartIndent/SmartIndent.cs#L39
        public static int FindTotalParentChainIndent(
            SyntaxNodeBase node, 
            int position, 
            int indent, 
            int indentSize,
            IIndentationService indentationService,
            ISyntaxFactsService syntaxFactsService)
        {
            var textSpanOpt = syntaxFactsService.GetFileSpanRoot(node);
            if (textSpanOpt == null)
                return indent;

            var textSpan = textSpanOpt.Value;

            if (!textSpan.IsInRootFile)
                return indent;

            if (position < textSpan.Span.Start || position > textSpan.Span.End)
                return indent;

            foreach (var child in node.ChildNodes)
            {
                var childSpan = syntaxFactsService.GetFileSpanRoot(child);
                if (childSpan == null || !childSpan.Value.IsInRootFile)
                    continue;

                var shouldIndent = indentationService.ShouldIndent(child);
                if (shouldIndent)
                    indent += indentSize;

                if (position <= childSpan.Value.Span.End)
                    return FindTotalParentChainIndent(child, position, indent, indentSize, indentationService, syntaxFactsService);

                if (shouldIndent)
                    indent -= indentSize;
            }

            return indent;
        }

        private int? DoBlockIndent(ITextSnapshotLine line, DocumentOptionSet optionSet)
        {
            for (var lineNumber = line.LineNumber - 1; lineNumber >= 0; --lineNumber)
            {
                var previousLine = line.Snapshot.GetLineFromLineNumber(lineNumber);

                string text = previousLine.GetText();

                if (text.Length > 0)
                {
                    return GetLeadingWhiteSpace(text, optionSet);
                }
            }

            return null;
        }

        private int GetLeadingWhiteSpace(string text, DocumentOptionSet optionSet)
        {
            var size = 0;
            foreach (var ch in text)
            {
                if (ch == '\t')
                    size += optionSet.GetOption(FormattingOptions.TabSize);
                else if (ch == ' ')
                    size++;
                else
                    break;
            }

            return size;
        }
    }
}