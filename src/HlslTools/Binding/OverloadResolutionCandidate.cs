using System.Collections.Generic;
using System.Collections.Immutable;
using HlslTools.Binding.Signatures;
using HlslTools.Symbols;

namespace HlslTools.Binding
{
    internal sealed class OverloadResolutionCandidate<T>
        where T : Signature
    {
        internal OverloadResolutionCandidate(T signature, IEnumerable<TypeSymbol> argumentTypes, IEnumerable<Conversion> argumentConversions)
            : this(signature, argumentTypes, argumentConversions, false, int.MaxValue)
        {
        }

        private OverloadResolutionCandidate(T signature, IEnumerable<TypeSymbol> argumentTypes, IEnumerable<Conversion> argumentConversions, bool isApplicable, int score)
        {
            Signature = signature;
            ArgumentTypes = argumentTypes.ToImmutableArray();
            ArgumentConversions = argumentConversions.ToImmutableArray();
            IsApplicable = isApplicable;
            Score = score;
        }

        public bool IsApplicable { get; }

        public int Score { get; }

        public T Signature { get; }

        public ImmutableArray<TypeSymbol> ArgumentTypes { get; }

        public ImmutableArray<Conversion> ArgumentConversions { get; }

        internal OverloadResolutionCandidate<T> MarkApplicable()
        {
            return new OverloadResolutionCandidate<T>(Signature, ArgumentTypes, ArgumentConversions, true, int.MaxValue);
        }

        internal OverloadResolutionCandidate<T> MarkNotApplicable()
        {
            return new OverloadResolutionCandidate<T>(Signature, ArgumentTypes, ArgumentConversions, false, int.MaxValue);
        }

        internal OverloadResolutionCandidate<T> MarkScore(int score)
        {
            return new OverloadResolutionCandidate<T>(Signature, ArgumentTypes, ArgumentConversions, true, score);
        }

        public override string ToString()
        {
            var type = !IsApplicable ? "Not Applicable" : $"Score: {Score}";
            return $"{Signature} [{type}]";
        }
    }
}