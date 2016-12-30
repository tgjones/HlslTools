using System;
using System.Collections.Generic;
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
            var runningTotal = 0;
            foreach (var fileSegment in _fileSegments)
            {
                if (fileSegment.Text.Filename == null && position < fileSegment.Start + fileSegment.Length)
                    return new SourceLocation(runningTotal + (position - fileSegment.Start));
                runningTotal += fileSegment.Length;
            }
            return new SourceLocation(runningTotal);
        }

        public SourceRange MapRootFileRange(TextSpan span)
        {
            return new SourceRange(MapRootFilePosition(span.Start), span.Length);
        }

        public FilePoint GetFilePoint(SourceLocation location)
        {
            // TODO: Move this validation to SourceLocation.
            if (location.Position < 0)
                throw new ArgumentOutOfRangeException(nameof(location));

            var lastFileSegment = _fileSegments[_fileSegments.Count - 1];
            if (location.Position > lastFileSegment.Start + lastFileSegment.Length)
                throw new ArgumentOutOfRangeException(nameof(location));

            foreach (var fileSegment in _fileSegments)
                if (location.Position < fileSegment.Start + fileSegment.Length)
                    return new FilePoint(fileSegment.Text, location.Position - fileSegment.Start);

            throw new InvalidOperationException();
        }

        /// <summary>
        /// If the specified <paramref name="range"/> can't be mapped to span in a single file,
        /// returns a clipped range that fits into a single file.
        /// </summary>
        public override TextSpan GetSourceTextSpan(SourceRange range)
        {
            var lastFileSegment = _fileSegments[_fileSegments.Count - 1];
            if (range.Start.Position > lastFileSegment.Start + lastFileSegment.Length)
                throw new ArgumentOutOfRangeException(nameof(range));

            foreach (var fileSegment in _fileSegments)
            {
                if (range.Start.Position < fileSegment.Start + fileSegment.Length)
                {
                    var length = (range.End.Position < fileSegment.Start + fileSegment.Length)
                        ? range.Length
                        : fileSegment.Start + fileSegment.Length - range.Start.Position;
                    return new TextSpan(fileSegment.Text, range.Start.Position - fileSegment.Start, length);
                }
            }

            throw new InvalidOperationException();
        }
    }
}