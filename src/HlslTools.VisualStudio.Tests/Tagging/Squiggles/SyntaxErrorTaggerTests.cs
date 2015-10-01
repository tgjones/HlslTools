using HlslTools.VisualStudio.ErrorList;
using HlslTools.VisualStudio.Options;
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
        private IErrorListHelper _errorListHelper;

        protected override void OnTestFixtureSetUp()
        {
            _errorListHelper = Substitute.For<IErrorListHelper>();
            base.OnTestFixtureSetUp();
        }

        protected override SyntaxErrorTagger CreateTagger(BackgroundParser backgroundParser, ITextSnapshot snapshot)
        {
            return new SyntaxErrorTagger(Substitute.For<ITextView>(), backgroundParser, _errorListHelper, new FakeOptionsService());
        }

        protected override bool MustCreateTagSpans => false;
    }
}