using System;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging
{
    internal static class AsyncTaggerUtility
    {
        public static ITagger<T> CreateTagger<TTagger, T>(ITextBuffer textBuffer, Func<TTagger> createCallback)
            where TTagger : IAsyncTagger
            where T : ITag
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(
                typeof(TTagger),
                () =>
                {
                    var tagger = createCallback();
                    Task.Run(async () =>
                    {
                        var snapshot = textBuffer.CurrentSnapshot;
                        var snapshotSyntaxTree = new SnapshotSyntaxTree(snapshot, snapshot.GetSyntaxTree(CancellationToken.None));
                        await tagger.InvalidateTags(snapshotSyntaxTree, CancellationToken.None);
                    });
                    return tagger as ITagger<T>;
                });
        }
    }
}