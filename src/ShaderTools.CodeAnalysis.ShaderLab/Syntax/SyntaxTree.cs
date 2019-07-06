using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class SyntaxTree : SyntaxTreeBase
    {
        private readonly SourceFile _sourceFile;

        public override SourceText Text => _sourceFile.Text;

        public override ParseOptions Options => null;

        public override SyntaxNodeBase Root { get; }

        internal SyntaxTree(SourceText text, Func<SyntaxTree, SyntaxNode> parseFunc)
        {
            _sourceFile = new SourceFile(text);
            Root = parseFunc(this);
        }

        public override IEnumerable<Diagnostic> GetDiagnostics()
        {
            return Root.GetDiagnostics();
        }

        public override SourceFileSpan GetSourceFileSpan(SourceRange range)
        {
            return new SourceFileSpan(
                _sourceFile, 
                new TextSpan(range.Start.Position, range.Length));
        }

        public int GetSourceFilePoint(SourceLocation location)
        {
            return location.Position;
        }

        public override SourceLocation MapRootFilePosition(int position)
        {
            return new SourceLocation(position);
        }

        public override SourceRange MapRootFileRange(TextSpan span)
        {
            return new SourceRange(new SourceLocation(span.Start), span.Length);
        }
    }
}