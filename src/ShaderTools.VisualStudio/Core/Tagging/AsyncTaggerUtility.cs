using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace ShaderTools.VisualStudio.Core.Tagging
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
                    Task.Run(async () => await tagger.InvalidateTags(textBuffer.CurrentSnapshot, CancellationToken.None));
                    return tagger as ITagger<T>;
                });
        }
    }
}