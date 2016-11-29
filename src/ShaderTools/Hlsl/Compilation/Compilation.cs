using System.Threading;
using ShaderTools.Hlsl.Binding;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Hlsl.Compilation
{
    public sealed class Compilation
    {
        public Compilation(SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }

        public SyntaxTree SyntaxTree { get; }

        public SemanticModel GetSemanticModel(CancellationToken? cancellationToken = null)
        {
            var bindingResult = Binder.Bind(SyntaxTree.Root, cancellationToken ?? CancellationToken.None);
            return new SemanticModel(this, bindingResult);
        }
    }
}