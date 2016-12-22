using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using NUnit.Framework;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Classification;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging.Classification
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