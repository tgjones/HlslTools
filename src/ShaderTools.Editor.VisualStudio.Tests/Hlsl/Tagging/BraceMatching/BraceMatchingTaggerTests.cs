using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using NSubstitute;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Tagging.BraceMatching;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging.BraceMatching
{
    public class BraceMatchingTaggerTests : AsyncTaggerTestsBase
    {
        [Theory]
        [MemberData(nameof(VsShaderTestUtility.GetTestShaders), MemberType = typeof(VsShaderTestUtility))]
        public async Task CanDoTagging(string testFile)
        {
            await RunTestAsync<BraceMatchingTagger, ITextMarkerTag>(testFile, CreateTagger);
        }

        private BraceMatchingTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
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