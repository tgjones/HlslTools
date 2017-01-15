using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Outlining;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging.Outlining
{
    internal class OutliningTaggerTests : AsyncTaggerTestsBase<OutliningTagger, IOutliningRegionTag>
    {
        [Theory]
        [MemberData(nameof(VsShaderTestUtility.GetTestShaders), MemberType = typeof(VsShaderTestUtility))]
        public async Task CanDoTagging(string testFile)
        {
            await RunTestAsync(testFile);
        }

        protected override OutliningTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            return new OutliningTagger(textBuffer, backgroundParser, new FakeOptionsService());
        }
    }
}