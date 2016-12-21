using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using NSubstitute;
using NUnit.Framework;
using ShaderTools.VisualStudio.Hlsl.Parsing;
using ShaderTools.VisualStudio.Hlsl.Tagging.Squiggles;
using ShaderTools.VisualStudio.Tests.Hlsl.Support;

namespace ShaderTools.VisualStudio.Tests.Hlsl.Tagging.Squiggles
{
    [TestFixture]
    internal class SyntaxErrorTaggerTests : AsyncTaggerTestsBase<SyntaxErrorTagger, IErrorTag>
    {
        protected override SyntaxErrorTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            var textView = Substitute.For<ITextView>();
            textView.TextSnapshot.Returns(textBuffer.CurrentSnapshot);

            return new SyntaxErrorTagger(
                textView, textBuffer, backgroundParser, new FakeOptionsService());
        }

        protected override bool MustCreateTagSpans => false;
    }
}
