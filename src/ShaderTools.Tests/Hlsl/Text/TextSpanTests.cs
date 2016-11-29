using NUnit.Framework;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Tests.Hlsl.Text
{
    // From https://github.com/terrajobst/nquery-vnext/blob/master/src/NQuery.Tests/Text/TextSpanTests.cs
    [TestFixture]
    public sealed class TextSpanTests
    {
        [Test]
        public void TextSpan_IntializeStartAndLength()
        {
            var textSpan = new TextSpan(SourceText.From(""), 1, 2);
            Assert.AreEqual(1, textSpan.Start);
            Assert.AreEqual(2, textSpan.Length);
            Assert.AreEqual(3, textSpan.End);
        }

        [Test]
        public void TextSpan_IntializeStartAndEnd()
        {
            var textSpan = TextSpan.FromBounds(SourceText.From(""), 1, 3);
            Assert.AreEqual(1, textSpan.Start);
            Assert.AreEqual(2, textSpan.Length);
            Assert.AreEqual(3, textSpan.End);
        }

        [Test]
        public void TextSpan_ContainsStart()
        {
            var textSpan = new TextSpan(SourceText.From(""), 1, 2);
            Assert.True(textSpan.Contains(textSpan.Start));
        }

        [Test]
        public void TextSpan_ContainsItself()
        {
            var textSpan = new TextSpan(SourceText.From(""), 1, 2);
            Assert.True(textSpan.Contains(textSpan));
        }

        [Test]
        public void TextSpan_DoesNotContainEnd()
        {
            var textSpan = new TextSpan(SourceText.From(""), 1, 2);
            Assert.False(textSpan.Contains(textSpan.End));
        }

        [Test]
        public void TextSpan_ContainsOrTouchesStart()
        {
            var textSpan = new TextSpan(SourceText.From(""), 1, 2);
            Assert.True(textSpan.ContainsOrTouches(textSpan.Start));
        }

        [Test]
        public void TextSpan_ContainsOrTouchesEnd()
        {
            var textSpan = new TextSpan(SourceText.From(""), 1, 2);
            Assert.True(textSpan.ContainsOrTouches(textSpan.End));
        }

        [Test]
        public void TextSpan_OverlapsWithSelf()
        {
            var textSpan = new TextSpan(SourceText.From(""), 1, 2);
            Assert.True(textSpan.OverlapsWith(textSpan));
        }

        [Test]
        public void TextSpan_OverlapsWhenOverhanging()
        {
            var textSpan1 = TextSpan.FromBounds(SourceText.From(""), 2, 5);
            var textSpan2 = TextSpan.FromBounds(SourceText.From(""), 3, 7);
            Assert.True(textSpan1.OverlapsWith(textSpan2));
            Assert.True(textSpan2.OverlapsWith(textSpan1));
        }

        [Test]
        public void TextSpan_OverlapsWhenContained()
        {
            var textSpan1 = TextSpan.FromBounds(SourceText.From(""), 2, 8);
            var textSpan2 = TextSpan.FromBounds(SourceText.From(""), 3, 6);
            Assert.True(textSpan1.OverlapsWith(textSpan2));
            Assert.True(textSpan2.OverlapsWith(textSpan1));
        }

        [Test]
        public void TextSpan_DoesNotOverlapAtEnd()
        {
            var textSpan1 = TextSpan.FromBounds(SourceText.From(""), 1, 2);
            var textSpan2 = TextSpan.FromBounds(SourceText.From(""), 2, 3);
            Assert.False(textSpan1.OverlapsWith(textSpan2));
            Assert.False(textSpan2.OverlapsWith(textSpan1));
        }

        [Test]
        public void TextSpan_DoesNotOverlapWithEmpty()
        {
            var empty = new TextSpan(SourceText.From(""), 1, 0);
            Assert.False(empty.OverlapsWith(empty));
        }

        [Test]
        public void TextSpan_IntersectsWithSelf()
        {
            var textSpan = new TextSpan(SourceText.From(""), 2, 4);
            Assert.True(textSpan.IntersectsWith(textSpan));
        }

        [Test]
        public void TextSpan_IntersectsWhenOverhanging()
        {
            var textSpan1 = TextSpan.FromBounds(SourceText.From(""), 2, 5);
            var textSpan2 = TextSpan.FromBounds(SourceText.From(""), 3, 7);
            Assert.True(textSpan1.IntersectsWith(textSpan2));
            Assert.True(textSpan2.IntersectsWith(textSpan1));
        }

        [Test]
        public void TextSpan_IntersectsWhenContained()
        {
            var textSpan1 = TextSpan.FromBounds(SourceText.From(""), 2, 8);
            var textSpan2 = TextSpan.FromBounds(SourceText.From(""), 3, 6);
            Assert.True(textSpan1.IntersectsWith(textSpan2));
            Assert.True(textSpan2.IntersectsWith(textSpan1));
        }

        [Test]
        public void TextSpan_IntersectsAtEnd()
        {
            var textSpan1 = TextSpan.FromBounds(SourceText.From(""), 1, 2);
            var textSpan2 = TextSpan.FromBounds(SourceText.From(""), 2, 3);
            Assert.True(textSpan1.IntersectsWith(textSpan2));
            Assert.True(textSpan2.IntersectsWith(textSpan1));
        }

        [Test]
        public void TextSpan_IntersectsWithEmpty()
        {
            var empty = new TextSpan(SourceText.From(""), 1, 0);
            Assert.True(empty.IntersectsWith(empty));
        }

        [Test]
        public void TextSpan_EqualWithSelf()
        {
            var textSpan1 = new TextSpan(SourceText.From(""), 1, 2);
            var textSpan2 = textSpan1;
            Assert.AreEqual(textSpan1, textSpan2);
            Assert.True(textSpan1 == textSpan2);
        }

        [Test]
        public void TextSpan_NotEqualWithOther()
        {
            var textSpan1 = new TextSpan(SourceText.From(""), 1, 2);
            var textSpan2 = new TextSpan(SourceText.From(""), 2, 3);
            Assert.AreNotEqual(textSpan1, textSpan2);
            Assert.True(textSpan1 != textSpan2);
        }

        [Test]
        public void TextSpan_ToStringIsReadable()
        {
            var textSpan = TextSpan.FromBounds(SourceText.From(""), 1, 3);
            Assert.AreEqual("[1,3)", textSpan.ToString());
        }
    }
}