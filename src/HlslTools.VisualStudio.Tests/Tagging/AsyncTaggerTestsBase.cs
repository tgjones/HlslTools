using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Tagging;
using HlslTools.VisualStudio.Tests.Support;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using NUnit.Framework;

namespace HlslTools.VisualStudio.Tests.Tagging
{
    internal abstract class AsyncTaggerTestsBase<TTagger, TTag> : MefTestsBase
        where TTagger : AsyncTagger<TTag>
        where TTag : ITag
    {
        [TestCaseSource(typeof(VsShaderTestUtility), nameof(VsShaderTestUtility.GetTestShaders))]
        public async Task CanDoTagging(string testFile)
        {
            // Arrange.
            var sourceCode = File.ReadAllText(testFile);
            var textBuffer = TextBufferUtility.CreateTextBuffer(Container, sourceCode);
            var backgroundParser = new BackgroundParser(textBuffer);
            var snapshot = textBuffer.CurrentSnapshot;
            var syntaxTree = snapshot.GetSyntaxTree(CancellationToken.None);
            var snapshotSyntaxTree = new SnapshotSyntaxTree(snapshot, syntaxTree);
            var tagger = CreateTagger(backgroundParser, snapshot);

            // Act.
            await tagger.InvalidateTags(snapshotSyntaxTree, CancellationToken.None);
            var tagSpans = tagger.GetTags(new NormalizedSnapshotSpanCollection(new[]
            {
                new SnapshotSpan(snapshot, 0, snapshot.Length)
            })).ToList();

            // Assert.
            if (MustCreateTagSpans)
                Assert.That(tagSpans.Any());

            backgroundParser.Dispose();
        }

        protected abstract TTagger CreateTagger(BackgroundParser backgroundParser, ITextSnapshot snapshot);

        protected virtual bool MustCreateTagSpans => true;
    }
}