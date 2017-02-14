using System.Collections.Generic;

namespace ShaderTools.Core.Text
{
    internal sealed class StringTextLineCollection : TextLineCollection
    {
        private readonly IReadOnlyList<TextLine> _lines;

        public StringTextLineCollection(IReadOnlyList<TextLine> lines)
        {
            _lines = lines;
        }

        public override IEnumerator<TextLine> GetEnumerator()
        {
            return _lines.GetEnumerator();
        }

        public override int Count
        {
            get { return _lines.Count; }
        }

        public override TextLine this[int index]
        {
            get { return _lines[index]; }
        }
    }
}