using System;
using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class SyntaxTree : SyntaxTreeBase
    {
        public override SourceText Text { get; }
        public SyntaxNode Root { get; }

        internal SyntaxTree(SourceText text, Func<SyntaxTree, SyntaxNode> parseFunc)
        {
            Text = text;
            Root = parseFunc(this);
        }

        public override IEnumerable<Diagnostic> GetDiagnostics()
        {
            return Root.GetDiagnostics();
        }

        public override TextSpan GetSourceTextSpan(SourceRange range)
        {
            return new TextSpan(Text, range.Start.Position, range.Length);
        }
    }
}