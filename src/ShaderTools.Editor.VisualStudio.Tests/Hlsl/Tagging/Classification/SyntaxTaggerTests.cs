using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging.Classification
{
#if false
    public class SyntaxTaggerTests : AsyncTaggerTestsBase
    {
        private readonly HlslClassificationService _hlslClassificationService;

        public SyntaxTaggerTests()
        {
            _hlslClassificationService = Container.GetExportedValue<HlslClassificationService>();
        }

        [Theory(Skip = "Need to update test")]
        //[HlslTestSuiteData]
        public async Task CanDoTagging(string testFile)
        {
            await RunTestAsync<SyntaxTagger, IClassificationTag>(testFile, CreateTagger);
        }

        private SyntaxTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            return new SyntaxTagger(_hlslClassificationService, backgroundParser);
        }
    }
#endif
}