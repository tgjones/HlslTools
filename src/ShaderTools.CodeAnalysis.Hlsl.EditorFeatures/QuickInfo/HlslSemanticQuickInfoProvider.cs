// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.QuickInfo;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.Hlsl.QuickInfo
{
    [ExportQuickInfoProvider(PredefinedQuickInfoProviderNames.Semantic, LanguageNames.Hlsl)]
    internal class HlslSemanticQuickInfoProvider : AbstractSemanticQuickInfoProvider
    {
        [ImportingConstructor]
        public HlslSemanticQuickInfoProvider(
            IProjectionBufferFactoryService projectionBufferFactoryService,
            IEditorOptionsFactoryService editorOptionsFactoryService,
            ITextEditorFactoryService textEditorFactoryService,
            IGlyphService glyphService,
            ClassificationTypeMap typeMap)
            : base(projectionBufferFactoryService, editorOptionsFactoryService,
                   textEditorFactoryService, glyphService, typeMap,
                   PrimaryWorkspace.Workspace.Services.GetLanguageServices(LanguageNames.Hlsl).GetRequiredService<ITaggedTextMappingService>())
        {
        }
    }
}
