using System.Collections.Generic;
using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Hlsl.Binding.Signatures;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding
{
    internal sealed class OverloadResolutionResult<T>
        where T : Signature
    {
        public OverloadResolutionResult(OverloadResolutionCandidate<T> best, OverloadResolutionCandidate<T> selected, IEnumerable<OverloadResolutionCandidate<T>> candidates)
        {
            Best = best;
            Selected = selected;
            Candidates = candidates.ToImmutableArray();
        }

        public static readonly OverloadResolutionResult<T> None = new OverloadResolutionResult<T>(null, null, new OverloadResolutionCandidate<T>[0]);

        public OverloadResolutionCandidate<T> Best { get; }
        public OverloadResolutionCandidate<T> Selected { get; }
        public ImmutableArray<OverloadResolutionCandidate<T>> Candidates { get; }
    }
}