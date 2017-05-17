using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;
using ShaderTools.Editor.VisualStudio.Hlsl.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions
{
    internal static class Extensions
    {
        private static readonly object IncludeFileSystemKey = new object();
        private static readonly object ConfigFileKey = new object();

        public static IIncludeFileSystem GetIncludeFileSystem(this ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(IncludeFileSystemKey,
                () => new VisualStudioFileSystem());
        }

        public static ConfigFile GetConfigFile(this ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(ConfigFileKey,
                () =>
                {
                    var filePath = textBuffer.GetTextDocument()?.FilePath;
                    if (filePath != null)
                        filePath = Path.GetDirectoryName(filePath);
                    return ConfigFileLoader.LoadAndMergeConfigFile(filePath);
                });
        }

        public static bool TryGetSemanticModel(this ITextSnapshot snapshot, CancellationToken cancellationToken, out SemanticModel semanticModel)
        {
            try
            {
                var document = snapshot.AsText().GetOpenDocumentInCurrentContextWithChanges();
                if (document == null)
                {
                    semanticModel = null;
                    return false;
                }
                var semanticModelTask = document.GetSemanticModelAsync(cancellationToken);
                semanticModelTask.Wait(cancellationToken);
                semanticModel = (SemanticModel) semanticModelTask.Result;
            }
            catch (OperationCanceledException)
            {
                semanticModel = null;
            }

            return semanticModel != null;
        }

        public static int? GetPosition(this ITextView syntaxEditor, ITextSnapshot snapshot)
        {
            var caretPoint = syntaxEditor.Caret.Position.BufferPosition;
            var snapshotPoint = syntaxEditor.BufferGraph.MapDownToSnapshot(caretPoint, PointTrackingMode.Positive, snapshot, PositionAffinity.Successor);
            if (snapshotPoint.HasValue)
                return snapshotPoint.Value.Position;
            return null;
        }

        // From https://github.com/dotnet/roslyn/blob/e39a3aeb1185ef0b349cad96a105969423065eac/src/EditorFeatures/Core/Shared/Extensions/ITextViewExtensions.cs#L278
        public static int? GetDesiredIndentation(this ITextView textView, ISmartIndentationService smartIndentService, ITextSnapshotLine line)
        {
            var pointInView = textView.BufferGraph.MapUpToSnapshot(line.Start, PointTrackingMode.Positive, PositionAffinity.Successor, textView.TextSnapshot);

            if (!pointInView.HasValue)
                return null;

            var lineInView = textView.TextSnapshot.GetLineFromPosition(pointInView.Value.Position);
            return smartIndentService.GetDesiredIndentation(textView, lineInView);
        }
    }
}