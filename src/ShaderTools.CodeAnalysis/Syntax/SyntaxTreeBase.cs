using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Syntax
{
    public abstract class SyntaxTreeBase
    {
        public abstract SourceText Text { get; }

        public abstract TextSpan GetSourceTextSpan(SourceRange range);
        public abstract IEnumerable<Diagnostic> GetDiagnostics();
    }
}
