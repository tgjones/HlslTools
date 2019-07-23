using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace ShaderTools.CodeAnalysis.Editor.Completion
{
    internal sealed class CompletionCommitManager : IAsyncCompletionCommitManager
    {
        public IEnumerable<char> PotentialCommitCharacters { get; } = Microsoft.CodeAnalysis.Completion.CompletionRules.Default.DefaultCommitCharacters;

        public bool ShouldCommitCompletion(char typedChar, SnapshotPoint location, CancellationToken token)
        {
            return true;
        }

        public CommitResult TryCommit(ITextView view, ITextBuffer buffer, CompletionItem item, ITrackingSpan applicableToSpan, char typedChar, CancellationToken token)
        {
            return CommitResult.Unhandled;
        }
    }
}
