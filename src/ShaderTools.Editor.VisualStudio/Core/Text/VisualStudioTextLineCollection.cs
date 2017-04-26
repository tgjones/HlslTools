using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Core.Text
{
    internal sealed class VisualStudioTextLineCollection : TextLineCollection
    {
        private readonly ITextSnapshot _snapshot;
        private readonly SourceText _sourceText;

        public VisualStudioTextLineCollection(SourceText sourceText, ITextSnapshot snapshot)
        {
            _sourceText = sourceText;
            _snapshot = snapshot;
        }

        public override IEnumerator<TextLine> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        public override int Count
        {
            get { return _snapshot.LineCount; }
        }

        public override TextLine this[int index]
        {
            get
            {
                var line = _snapshot.GetLineFromLineNumber(index);
                return new TextLine(_sourceText, line.Start, line.Length);
            }
        }
    }

}
