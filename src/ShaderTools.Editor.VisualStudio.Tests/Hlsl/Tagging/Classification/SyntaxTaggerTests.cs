using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Classification;
using ShaderTools.Testing.TestResources.Hlsl;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging.Classification
{
    public class SyntaxTaggerTests : AsyncTaggerTestsBase
    {
        private readonly HlslClassificationService _hlslClassificationService;

        public SyntaxTaggerTests()
        {
            _hlslClassificationService = Container.GetExportedValue<HlslClassificationService>();
        }

        [Theory]
        [HlslTestSuiteData]
        public async Task CanDoTagging(string testFile)
        {
            await RunTestAsync<SyntaxTagger, IClassificationTag>(testFile, CreateTagger);
        }

        private SyntaxTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            return new SyntaxTagger(_hlslClassificationService, backgroundParser);
        }
    }
}