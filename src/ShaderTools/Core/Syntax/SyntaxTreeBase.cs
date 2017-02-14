using System.Collections.Generic;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Text;

namespace ShaderTools.Core.Syntax
{
    public abstract class SyntaxTreeBase
    {
        public abstract SourceText Text { get; }

        public abstract TextSpan GetSourceTextSpan(SourceRange range);
        public abstract IEnumerable<Diagnostic> GetDiagnostics();
    }
}
