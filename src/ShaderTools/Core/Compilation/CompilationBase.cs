using System.Threading;
using ShaderTools.Core.Syntax;

namespace ShaderTools.Core.Compilation
{
    public abstract class CompilationBase
    {
        public abstract SyntaxTreeBase SyntaxTreeBase { get; }

        public abstract SemanticModelBase GetSemanticModelBase(CancellationToken cancellationToken);
    }
}
