using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;

namespace ShaderTools.CodeAnalysis.Editor.Completion
{
    internal sealed class CompletionCommitManager : IAsyncCompletionCommitManager
    {
        public IEnumerable<char> PotentialCommitCharacters { get; } = Microsoft.CodeAnalysis.Completion.CompletionRules.Default.DefaultCommitCharacters;

        public bool ShouldCommitCompletion(IAsyncCompletionSession session, SnapshotPoint location, char typedChar, CancellationToken token)
        {
            return true;
        }

        public CommitResult TryCommit(IAsyncCompletionSession session, ITextBuffer buffer, CompletionItem item, char typedChar, CancellationToken token)
        {
            return CommitResult.Unhandled;
        }
    }
}
