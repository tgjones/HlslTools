using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using NUnit.Framework;
using ShaderTools.VisualStudio.Hlsl.Parsing;
using ShaderTools.VisualStudio.Hlsl.Tagging.Classification;

namespace ShaderTools.VisualStudio.Tests.Hlsl.Tagging.Classification
{
    [TestFixture]
    internal class SyntaxTaggerTests : AsyncTaggerTestsBase<SyntaxTagger, IClassificationTag>
    {
        private HlslClassificationService _hlslClassificationService;

        protected override void OnTestFixtureSetUp()
        {
            _hlslClassificationService = Container.GetExportedValue<HlslClassificationService>();
        }

        protected override SyntaxTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            return new SyntaxTagger(_hlslClassificationService, backgroundParser);
        }
    }
}