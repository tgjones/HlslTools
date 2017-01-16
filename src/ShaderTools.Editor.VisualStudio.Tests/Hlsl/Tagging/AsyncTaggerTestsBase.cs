using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Editor.VisualStudio.Core.Tagging;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Text;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging
{
    public abstract class AsyncTaggerTestsBase : MefTestsBase
    {
        internal async Task RunTestAsync<TTagger, TTag>(string testFile, CreateTagger<TTagger, TTag> createTagger)
            where TTagger : AsyncTagger<TTag>
            where TTag : ITag
        {
            // Arrange.
            VisualStudioSourceTextFactory.Instance = Container.GetExportedValue<VisualStudioSourceTextFactory>();
            var sourceCode = File.ReadAllText(testFile);
            var textBuffer = TextBufferUtility.CreateTextBuffer(Container, sourceCode);
            var backgroundParser = new BackgroundParser(textBuffer);
            var snapshot = textBuffer.CurrentSnapshot;
            var tagger = createTagger(backgroundParser, textBuffer);

            // Act.
            await tagger.InvalidateTags(snapshot, CancellationToken.None);
            var tagSpans = tagger.GetTags(new NormalizedSnapshotSpanCollection(new[]
            {
                new SnapshotSpan(snapshot, 0, snapshot.Length)
            })).ToList();

            // Assert.
            if (MustCreateTagSpans)
                Assert.NotEmpty(tagSpans);

            backgroundParser.Dispose();
        }

        internal delegate TTagger CreateTagger<TTagger, TTag>(BackgroundParser backgroundParser, ITextBuffer textBuffer)
            where TTagger : AsyncTagger<TTag>
            where TTag : ITag;

        protected virtual bool MustCreateTagSpans => true;
    }
}