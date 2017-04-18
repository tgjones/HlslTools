using System;
using System.Threading;
using ShaderTools.Core.Compilation;
using ShaderTools.Core.Syntax;
using ShaderTools.Hlsl.Binding;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Hlsl.Compilation
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