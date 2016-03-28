using System;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Text;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging
{
    internal static class AsyncTaggerUtility
    {
        public static ITagger<T> CreateTagger<TTagger, T>(ITextBuffer textBuffer, Func<TTagger> createCallback, VisualStudioSourceTextFactory sourceTextFactory)
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
                        var snapshotSyntaxTree = new SnapshotSyntaxTree(snapshot,
                            snapshot.GetSyntaxTree(sourceTextFactory, CancellationToken.None),
                            snapshot.GetSemanticModel(sourceTextFactory, CancellationToken.None));
                        await tagger.InvalidateTags(snapshotSyntaxTree, CancellationToken.None);
                    });
                    return tagger as ITagger<T>;
                });
        }
    }
}