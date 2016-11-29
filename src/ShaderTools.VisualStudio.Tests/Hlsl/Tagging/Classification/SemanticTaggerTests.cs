using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using NUnit.Framework;
using ShaderTools.VisualStudio.Hlsl.Parsing;
using ShaderTools.VisualStudio.Hlsl.Tagging.Classification;

namespace ShaderTools.VisualStudio.Tests.Hlsl.Tagging.Classification
{
    [TestFixture]
    internal class SemanticTaggerTests : AsyncTaggerTestsBase<SemanticTagger, IClassificationTag>
    {
        private HlslClassificationService _hlslClassificationService;

        protected override void OnTestFixtureSetUp()
        {
            _hlslClassificationService = Container.GetExportedValue<HlslClassificationService>();
        }

        protected override SemanticTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            return new SemanticTagger(_hlslClassificationService, backgroundParser);
        }
    }
}