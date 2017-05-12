using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Editor.VisualStudio.Hlsl.Text;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging
{
#if false
    public abstract class AsyncTaggerTestsBase : MefTestsBase
    {
        internal async Task RunTestAsync<TTagger, TTag>(string testFile, CreateTagger<TTagger, TTag> createTagger)
            where TTagger : AsyncTagger<TTag>
            where TTag : ITag
        {
            var tagSpans = await GetTagSpans(File.ReadAllText(testFile), createTagger);

            // Assert.
            if (MustCreateTagSpans)
                Assert.NotEmpty(tagSpans);
        }

        internal async Task<List<ITagSpan<TTag>>> GetTagSpans<TTagger, TTag>(string sourceCode, CreateTagger<TTagger, TTag> createTagger)
            where TTagger : AsyncTagger<TTag>
            where TTag : ITag
        {
            var textBuffer = TextBufferUtility.CreateTextBuffer(Container, sourceCode);
            var backgroundParser = new BackgroundParser(textBuffer);
            var snapshot = textBuffer.CurrentSnapshot;
            var tagger = createTagger(backgroundParser, textBuffer);

            await tagger.InvalidateTags(snapshot, CancellationToken.None);
            var tags = tagger.GetTags(new NormalizedSnapshotSpanCollection(new[]
            {
                new SnapshotSpan(snapshot, 0, snapshot.Length)
            })).ToList();

            backgroundParser.Dispose();

            return tags;
        }

        internal delegate TTagger CreateTagger<TTagger, TTag>(BackgroundParser backgroundParser, ITextBuffer textBuffer)
            where TTagger : AsyncTagger<TTag>
            where TTag : ITag;

        protected virtual bool MustCreateTagSpans => true;
    }
#endif
}