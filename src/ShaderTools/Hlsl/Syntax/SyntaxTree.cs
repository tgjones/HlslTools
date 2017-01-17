using System;
using System.Collections.Generic;
using System.Linq;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Parser;

namespace ShaderTools.Hlsl.Syntax
{
    public sealed class SyntaxTree : SyntaxTreeBase
    {
        private readonly List<FileSegment> _fileSegments;

        public SourceText Text { get; }
        public SyntaxNode Root { get; }

        internal SyntaxTree(SourceText text, Func<SyntaxTree, Tuple<SyntaxNode, List<FileSegment>>> parseFunc)
        {
            Text = text;

            var parsed = parseFunc(this);
            Root = parsed.Item1;
            _fileSegments = parsed.Item2;
        }

        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return Root.GetDiagnostics();
        }

        public SourceLocation MapRootFilePosition(int position)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            var lastRootFileSegment = _fileSegments.FindLast(x => x.Text.IsRoot);
            if (position > lastRootFileSegment.Start + lastRootFileSegment.Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            var runningTotal = 0;
            foreach (var fileSegment in _fileSegments)
            {
                if (fileSegment.Text.IsRoot && position < fileSegment.Start + fileSegment.Length)
                    return new SourceLocation(runningTotal + (position - fileSegment.Start));
                runningTotal += fileSegment.Length;
            }
            return new SourceLocation(runningTotal);
        }

        public SourceRange MapRootFileRange(TextSpan span)
        {
            return new SourceRange(MapRootFilePosition(span.Start), span.Length);
        }

        /// <summary>
        /// If the specified <paramref name="range"/> can't be mapped to span in a single file,
        /// returns a clipped range that fits into a single file (segment).
        /// </summary>
        public override TextSpan GetSourceTextSpan(SourceRange range)
        {
            if (range.Start.Position < 0)
                throw new ArgumentOutOfRangeException(nameof(range));

            var lastFileSegment = _fileSegments[_fileSegments.Count - 1];
            if (range.Start.Position > _fileSegments.Sum(x => x.Length))
                throw new ArgumentOutOfRangeException(nameof(range));

            var runningTotal = 0;
            foreach (var fileSegment in _fileSegments)
            {
                if (range.Start.Position < runningTotal + fileSegment.Length || fileSegment == lastFileSegment)
                {
                    var length = (range.End.Position < runningTotal + fileSegment.Length)
                        ? range.Length
                        : runningTotal + fileSegment.Length - range.Start.Position;
                    return new TextSpan(fileSegment.Text, range.Start.Position - runningTotal + fileSegment.Start, length);
                }
                runningTotal += fileSegment.Length;
            }

            throw new InvalidOperationException();
        }
    }
}