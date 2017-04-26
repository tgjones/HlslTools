using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;

namespace ShaderTools.CodeAnalysis.Compilation
{
    public abstract class SemanticModelBase
    {
        public abstract IEnumerable<Diagnostic> GetDiagnostics();
    }
}
