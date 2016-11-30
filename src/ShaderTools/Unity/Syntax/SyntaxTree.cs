using System;
using System.Collections.Generic;
using ShaderTools.Core.Text;
using ShaderTools.Unity.Diagnostics;

namespace ShaderTools.Unity.Syntax
{
    public sealed class SyntaxTree
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
    }
}