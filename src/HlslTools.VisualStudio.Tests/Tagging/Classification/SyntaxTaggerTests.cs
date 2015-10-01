using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Tagging.Classification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using NUnit.Framework;

namespace HlslTools.VisualStudio.Tests.Tagging.Classification
{
    [TestFixture]
    internal class SyntaxTaggerTests : AsyncTaggerTestsBase<SyntaxTagger, IClassificationTag>
    {
        private HlslClassificationService _hlslClassificationService;

        protected override void OnTestFixtureSetUp()
        {
            _hlslClassificationService = Container.GetExportedValue<HlslClassificationService>();
        }

        protected override SyntaxTagger CreateTagger(BackgroundParser backgroundParser, ITextSnapshot snapshot)
        {
            return new SyntaxTagger(_hlslClassificationService, backgroundParser);
        }
    }
}