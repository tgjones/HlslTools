using System.Threading;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Compilation
{
    public abstract class CompilationBase
    {
        public abstract SyntaxTreeBase SyntaxTreeBase { get; }

        public abstract SemanticModelBase GetSemanticModelBase(CancellationToken cancellationToken);
    }
}
