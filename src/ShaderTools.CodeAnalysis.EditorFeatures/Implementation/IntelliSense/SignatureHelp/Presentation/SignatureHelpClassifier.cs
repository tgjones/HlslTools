// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.SignatureHelp.Presentation
{
    internal class SignatureHelpClassifier : IClassifier
    {
        private readonly ITextBuffer _subjectBuffer;
        private readonly ClassificationTypeMap _typeMap;
        private readonly ITaggedTextMappingService _taggedTextMappingService;

#pragma warning disable 67
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
#pragma warning restore 67

        public SignatureHelpClassifier(ITextBuffer subjectBuffer, ClassificationTypeMap typeMap)
        {
            _subjectBuffer = subjectBuffer;
            _typeMap = typeMap;
            _taggedTextMappingService = PrimaryWorkspace.Workspace.Services.GetLanguageServices(LanguageNames.Hlsl).GetRequiredService<ITaggedTextMappingService>();
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            if (_subjectBuffer.Properties.TryGetProperty(typeof(ISignatureHelpSession), out ISignatureHelpSession session) &&
                session.SelectedSignature is Signature)
            {
                var signature = (Signature) session.SelectedSignature;
                if (!_subjectBuffer.Properties.TryGetProperty("UsePrettyPrintedContent", out bool usePrettyPrintedContent))
                {
                    usePrettyPrintedContent = false;
                }

                var content = usePrettyPrintedContent
                    ? signature.PrettyPrintedContent
                    : signature.Content;

                var displayParts = usePrettyPrintedContent
                    ? signature.PrettyPrintedDisplayParts
                    : signature.DisplayParts;

                if (content == _subjectBuffer.CurrentSnapshot.GetText())
                {
                    return displayParts.ToClassificationSpans(span.Snapshot, _typeMap, _taggedTextMappingService);
                }
            }

            return SpecializedCollections.EmptyList<ClassificationSpan>();
        }
    }
}
