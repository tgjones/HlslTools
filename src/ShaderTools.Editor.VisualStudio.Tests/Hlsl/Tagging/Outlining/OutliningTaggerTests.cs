using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Outlining;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging.Outlining
{
    public class OutliningTaggerTests : AsyncTaggerTestsBase
    {
        [Theory]
        [MemberData(nameof(VsShaderTestUtility.GetTestShaders), MemberType = typeof(VsShaderTestUtility))]
        public async Task CanDoTagging(string testFile)
        {
            await RunTestAsync<OutliningTagger, IOutliningRegionTag>(testFile, CreateTagger);
        }

        private OutliningTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            return new OutliningTagger(textBuffer, backgroundParser, new FakeOptionsService());
        }
    }
}