using System;
using Microsoft.VisualStudio.Text;
using ShaderTools.Core.Text;

namespace ShaderTools.Editor.VisualStudio.Core.Text
{
    internal sealed class VisualStudioSourceText : SourceText
    {
        public VisualStudioSourceText(ITextSnapshot snapshot, string filename, bool isRoot)
        {
            Snapshot = snapshot;
            Length = Snapshot.Length;
            Filename = filename;
            IsRoot = isRoot;

            Lines = new VisualStudioTextLineCollection(this, snapshot);
        }

        public override string GetText(TextSpan textSpan)
        {
            return Snapshot.GetText(textSpan.Start, textSpan.Length);
        }

        public ITextSnapshot Snapshot { get; }

        public override char this[int index] => Snapshot[index];

        public override int Length { get; }

        public override TextLineCollection Lines { get; }

        public override string Filename { get; }

        public override int GetLineNumberFromPosition(int position)
        {
            if (position < 0 || position > Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            return Snapshot.GetLineNumberFromPosition(position);
        }

        public override bool IsRoot { get; }
    }
}