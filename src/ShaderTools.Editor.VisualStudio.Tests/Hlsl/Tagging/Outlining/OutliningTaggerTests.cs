using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;
using ShaderTools.Editor.VisualStudio.Tests.Support;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging.Outlining
{
#if false
    public class OutliningTaggerTests : AsyncTaggerTestsBase
    {
        [WpfFact]
        public async Task CreateOutlineTags()
        {
            var code =
@"namespace MyNamespace
{
    class MyClass
    {
        static void Main(int arg)
        {
            int x = 5;
        }
    };
}";

            var tagSpans = await GetTagSpans<OutliningTagger, IOutliningRegionTag>(code, CreateTagger);
            
            // ensure all 3 outlining region tags were found
            Assert.Equal(3, tagSpans.Count);

            // ensure only the method is marked as implementation
            Assert.False(tagSpans[0].Tag.IsImplementation);
            Assert.False(tagSpans[1].Tag.IsImplementation);
            Assert.True(tagSpans[2].Tag.IsImplementation);

            Assert.All(tagSpans.Select(x => x.Tag.CollapsedHintForm), x => Assert.IsType<string>(x));

            // verify line counts
            Assert.Equal(10, GetLineCount(tagSpans[0].Span)); // namespace
            Assert.Equal(7, GetLineCount(tagSpans[1].Span)); // class
            Assert.Equal(4, GetLineCount(tagSpans[2].Span)); // method
        }

        private int GetLineCount(SnapshotSpan span)
        {
            var startLine = span.Start.GetContainingLine().LineNumber;
            var endLine = span.End.GetContainingLine().LineNumber;

            return endLine - startLine + 1;
        }

        private OutliningTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            return new OutliningTagger(textBuffer, backgroundParser, new FakeOptionsService());
        }
    }
#endif
}