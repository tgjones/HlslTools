using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using NSubstitute;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging.Squiggles
{
#if false
    public class SyntaxErrorTaggerTests : AsyncTaggerTestsBase
    {
        [Theory(Skip = "Need to update test")]
        //[HlslTestSuiteData]
        public async Task CanDoTagging(string testFile)
        {
            await RunTestAsync<SyntaxErrorTagger, IErrorTag>(testFile, CreateTagger);
        }

        private SyntaxErrorTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            var textView = Substitute.For<ITextView>();
            textView.TextSnapshot.Returns(textBuffer.CurrentSnapshot);

            return new SyntaxErrorTagger(
                textView, textBuffer, backgroundParser, new FakeOptionsService());
        }

        protected override bool MustCreateTagSpans => false;
    }
#endif
}
