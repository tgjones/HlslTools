using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Tagging.Outlining;
using HlslTools.VisualStudio.Tests.Support;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using NUnit.Framework;

namespace HlslTools.VisualStudio.Tests.Tagging.Outlining
{
    [TestFixture]
    internal class OutliningTaggerTests : AsyncTaggerTestsBase<OutliningTagger, IOutliningRegionTag>
    {
        protected override OutliningTagger CreateTagger(BackgroundParser backgroundParser, ITextSnapshot snapshot)
        {
            return new OutliningTagger(backgroundParser, new FakeOptionsService());
        }
    }
}