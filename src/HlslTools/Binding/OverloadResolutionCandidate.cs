using System.Collections.Generic;
using System.Collections.Immutable;
using HlslTools.Binding.Signatures;
using HlslTools.Compilation;

namespace HlslTools.Binding
{
    internal sealed class OverloadResolutionCandidate<T>
        where T : Signature
    {
        public OverloadResolutionCandidate(T signature, IEnumerable<Conversion> argumentConversions, ulong score)
        {
            Signature = signature;
            ArgumentConversions = argumentConversions.ToImmutableArray();
            Score = score;
        }

        public ulong Score { get; }

        public T Signature { get; }

        public ImmutableArray<Conversion> ArgumentConversions { get; }

        public override string ToString()
        {
            var type = $"Score: {Score}";
            return $"{Signature} [{type}]";
        }
    }
}