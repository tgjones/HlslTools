using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions
{
    internal static class Extensions
    {
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
    }
}