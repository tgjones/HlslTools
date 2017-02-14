using System.Collections.Generic;
using ShaderTools.Core.Diagnostics;

namespace ShaderTools.Core.Compilation
{
    public abstract class SemanticModelBase
    {
        public abstract IEnumerable<Diagnostic> GetDiagnostics();
    }
}
