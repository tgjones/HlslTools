using System;
using System.Collections.Generic;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;

namespace ShaderTools.Unity.Syntax
{
    public sealed class SyntaxTree : SyntaxTreeBase
    {
        public SourceText Text { get; }
        public SyntaxNode Root { get; }

        internal SyntaxTree(SourceText text, Func<SyntaxTree, SyntaxNode> parseFunc)
        {
            Text = text;
            Root = parseFunc(this);
        }

        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return Root.GetDiagnostics();
        }

        public override TextSpan GetSourceTextSpan(SourceRange range)
        {
            return new TextSpan(Text, range.Start.Position, range.Length);
        }
    }
}