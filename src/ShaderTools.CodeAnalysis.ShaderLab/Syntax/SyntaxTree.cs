using System;
using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class SyntaxTree : SyntaxTreeBase
    {
        private readonly SourceFile _sourceFile;

        public override SourceText Text => _sourceFile.Text;

        public SyntaxNode Root { get; }

        internal SyntaxTree(SourceText text, Func<SyntaxTree, SyntaxNode> parseFunc)
        {
            _sourceFile = new SourceFile(text, null);
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
    }
}