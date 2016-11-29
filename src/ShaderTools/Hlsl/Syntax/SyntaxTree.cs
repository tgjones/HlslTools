using System;
using System.Collections.Generic;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Parser;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Hlsl.Syntax
{
    public sealed class SyntaxTree
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
    }
}