using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Tagging.BraceMatching;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using NSubstitute;
using NUnit.Framework;

namespace HlslTools.VisualStudio.Tests.Tagging.BraceMatching
{
    [TestFixture]
    internal class BraceMatchingTaggerTests : AsyncTaggerTestsBase<BraceMatchingTagger, ITextMarkerTag>
    {
        protected override BraceMatchingTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            var textView = Substitute.For<ITextView>();
            var snapshotPoint = new SnapshotPoint(textBuffer.CurrentSnapshot, 0);
            var virtualSnapshotPoint = new VirtualSnapshotPoint(snapshotPoint);
            textView.Selection.Start.Returns(virtualSnapshotPoint);
            textView.Selection.End.Returns(virtualSnapshotPoint);

            var mappingPoint = Substitute.For<IMappingPoint>();
            textView.Caret.Position.Returns(new CaretPosition(virtualSnapshotPoint, mappingPoint, PositionAffinity.Successor));

            return new BraceMatchingTagger(backgroundParser, textView, new BraceMatcher());
        }

        protected override bool MustCreateTagSpans => false;
    }
}