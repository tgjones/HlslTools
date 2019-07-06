using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Syntax
{
    public abstract class SyntaxTreeBase
    {
        public abstract SourceText Text { get; }

        public abstract ParseOptions Options { get; }

        public abstract SyntaxNodeBase Root { get; }

        public abstract SourceFileSpan GetSourceFileSpan(SourceRange range);
        public abstract IEnumerable<Diagnostic> GetDiagnostics();

        public abstract SourceLocation MapRootFilePosition(int position);
        public abstract SourceRange MapRootFileRange(TextSpan span);
    }
}
