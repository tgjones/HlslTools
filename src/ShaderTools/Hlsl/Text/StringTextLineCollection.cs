using System.Collections;
using System.Collections.Generic;

namespace ShaderTools.Hlsl.Text
{
    internal sealed class StringTextLineCollection : IReadOnlyList<TextLine>
    {
        private readonly IReadOnlyList<TextLine> _lines;

        public StringTextLineCollection(IReadOnlyList<TextLine> lines)
        {
            _lines = lines;
        }

        public IEnumerator<TextLine> GetEnumerator()
        {
            return _lines.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _lines.Count;

        public TextLine this[int index] => _lines[index];
    }
}