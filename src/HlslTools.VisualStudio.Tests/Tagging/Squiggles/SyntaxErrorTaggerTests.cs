using System;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Tagging.Squiggles;
using HlslTools.VisualStudio.Tests.Support;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using NSubstitute;
using NUnit.Framework;

namespace HlslTools.VisualStudio.Tests.Tagging.Squiggles
{
    [TestFixture]
    internal class SyntaxErrorTaggerTests : AsyncTaggerTestsBase<SyntaxErrorTagger, IErrorTag>
    {
        protected override SyntaxErrorTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            var textView = Substitute.For<ITextView>();
            textView.TextSnapshot.Returns(textBuffer.CurrentSnapshot);

            return new SyntaxErrorTagger(
                textView, backgroundParser, new FakeOptionsService());
        }

        protected override bool MustCreateTagSpans => false;
    }
}
