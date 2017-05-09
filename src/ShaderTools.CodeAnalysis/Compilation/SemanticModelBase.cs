using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Compilation
{
    public abstract class SemanticModelBase
    {
        public abstract SyntaxTreeBase SyntaxTree { get; }

        public abstract IEnumerable<Diagnostic> GetDiagnostics();
    }
}
