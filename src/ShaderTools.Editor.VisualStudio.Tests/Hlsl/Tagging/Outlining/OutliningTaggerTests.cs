using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using NUnit.Framework;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Outlining;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Tagging.Outlining
{
    [TestFixture]
    internal class OutliningTaggerTests : AsyncTaggerTestsBase<OutliningTagger, IOutliningRegionTag>
    {
        protected override OutliningTagger CreateTagger(BackgroundParser backgroundParser, ITextBuffer textBuffer)
        {
            return new OutliningTagger(textBuffer, backgroundParser, new FakeOptionsService());
        }
    }
}