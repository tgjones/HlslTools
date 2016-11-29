using System.Collections.Generic;
using System.Threading;
using ShaderTools.Hlsl.Binding.BoundNodes;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Hlsl.Binding
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