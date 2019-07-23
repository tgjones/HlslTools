using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Extensions
{
    internal static class WorkspaceExtensions
    {
        /// <summary>
        /// Update the workspace so that the document with the Id of <paramref name="newDocument"/>
        /// has the text of newDocument.  If the document is open, then this method will determine a
        /// minimal set of changes to apply to the document.
        /// </summary>
        internal static void ApplyDocumentChanges(this Workspace workspace, Document newDocument, CancellationToken cancellationToken)
        {
            var oldSolution = workspace.CurrentDocuments;
            var oldDocument = oldSolution.GetDocument(newDocument.Id);
            var changes = newDocument.GetTextChangesAsync(oldDocument, cancellationToken).WaitAndGetResult(cancellationToken);
            var newSolution = oldSolution.UpdateDocument(newDocument.Id, changes, cancellationToken);
            workspace.TryApplyChanges(newSolution);
        }

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
