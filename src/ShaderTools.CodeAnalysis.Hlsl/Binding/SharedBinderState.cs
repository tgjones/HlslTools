using System.Collections.Generic;
using System.Threading;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding
{
    internal sealed class SharedBinderState
    {
        public CancellationToken CancellationToken { get; }
        public Dictionary<SyntaxNode, BoundNode> BoundNodeFromSyntaxNode { get; } = new Dictionary<SyntaxNode, BoundNode>();
        public Dictionary<BoundNode, Binder> BinderFromBoundNode { get; } = new Dictionary<BoundNode, Binder>();
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        public SharedBinderState(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }
    }
}