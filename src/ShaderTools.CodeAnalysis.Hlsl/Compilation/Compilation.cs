using System.Threading;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Binding;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Compilation
{
    public sealed class Compilation : CompilationBase
    {
        public Compilation(SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }

        public SyntaxTree SyntaxTree { get; }

        public override SyntaxTreeBase SyntaxTreeBase => SyntaxTree;

        public SemanticModel GetSemanticModel(CancellationToken? cancellationToken = null)
        {
            var bindingResult = Binder.Bind(SyntaxTree.Root, cancellationToken ?? CancellationToken.None);
            return new SemanticModel(this, bindingResult);
        }

        public override SemanticModelBase GetSemanticModelBase(CancellationToken cancellationToken) => GetSemanticModel(cancellationToken);
    }
}