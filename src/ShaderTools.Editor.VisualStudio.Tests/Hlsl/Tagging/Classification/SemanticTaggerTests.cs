using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Classification;
using ShaderTools.Testing.TestResources.Hlsl;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging.Classification
{
    public class SemanticTaggerTests : AsyncTaggerTestsBase
    {
        private readonly HlslClassificationService _hlslClassificationService;

        public SemanticTaggerTests()
        {
            _hlslClassificationService = Container.GetExportedValue<HlslClassificationService>();
        }

        [Theory]
        [HlslTestSuiteData]
        public async Task CanDoTagging(string testFile)
        {
            await RunTestAsync<SemanticTagger, IClassificationTag>(testFile, CreateTagger);
        }

        private SemanticTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            return new SemanticTagger(_hlslClassificationService, backgroundParser);
        }
    }
}