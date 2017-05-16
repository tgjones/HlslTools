using System.Collections.Generic;
using System.Threading;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Extensions
{
    internal static class WorkspaceExtensions
    {
        /// <summary>
        /// Update the solution so that the document with the Id has the text changes
        /// </summary>
        internal static void ApplyTextChanges(this Workspace workspace, DocumentId id, IEnumerable<TextChange> textChanges, CancellationToken cancellationToken)
        {
            var oldSolution = workspace.CurrentDocuments;
            var newSolution = oldSolution.UpdateDocument(id, textChanges, cancellationToken);
            workspace.TryApplyChanges(newSolution);
        }

        internal static WorkspaceDocuments UpdateDocument(this WorkspaceDocuments solution, DocumentId id, IEnumerable<TextChange> textChanges, CancellationToken cancellationToken)
        {
            var oldDocument = solution.GetDocument(id);
            var oldText = oldDocument.SourceText;
            var newText = oldText.WithChanges(textChanges);
            return solution.WithDocumentText(id, newText);
        }
    }
}
