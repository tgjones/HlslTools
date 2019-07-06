using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Parser;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed class SyntaxTree : SyntaxTreeBase
    {
        private readonly List<FileSegment> _fileSegments;

        // TODO: Don't need both File and Text. And it should be renamed to RootFile.
        public SourceFile File { get; }
        public override SourceText Text { get; }
        public override SyntaxNodeBase Root { get; }

        public override ParseOptions Options { get; }

        internal SyntaxTree(SourceFile file, HlslParseOptions options, Func<SyntaxTree, Tuple<SyntaxNode, List<FileSegment>>> parseFunc)
        {
            File = file;
            Text = file.Text;
            Options = options;

            var parsed = parseFunc(this);
            Root = parsed.Item1;
            _fileSegments = parsed.Item2;
        }

        public override IEnumerable<Diagnostic> GetDiagnostics()
        {
            return Root.GetDiagnostics();
        }

        public override SourceLocation MapRootFilePosition(int position)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            var lastRootFileSegment = _fileSegments.FindLast(x => x.File.IsRootFile);
            if (position > lastRootFileSegment.Start + lastRootFileSegment.Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            var runningTotal = 0;
            foreach (var fileSegment in _fileSegments)
            {
                if (fileSegment.File.IsRootFile && position < fileSegment.Start + fileSegment.Length)
                    return new SourceLocation(runningTotal + (position - fileSegment.Start));
                runningTotal += fileSegment.Length;
            }
            return new SourceLocation(runningTotal);
        }

        public override SourceRange MapRootFileRange(TextSpan span)
        {
            return new SourceRange(MapRootFilePosition(span.Start), span.Length);
        }

        /// <summary>
        /// If the specified <paramref name="range"/> can't be mapped to span in a single file,
        /// returns a clipped range that fits into a single file (segment).
        /// </summary>
        public override SourceFileSpan GetSourceFileSpan(SourceRange range)
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

                    return new SourceFileSpan(
                        fileSegment.File, 
                        new TextSpan(
                            range.Start.Position - runningTotal + fileSegment.Start, 
                            length));
                }
                runningTotal += fileSegment.Length;
            }

            throw new InvalidOperationException();
        }
    }
}