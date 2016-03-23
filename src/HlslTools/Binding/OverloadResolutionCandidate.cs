using HlslTools.Binding.Signatures;

namespace HlslTools.Binding
{
    internal sealed class OverloadResolutionCandidate<T>
        where T : Signature
    {
        public OverloadResolutionCandidate(T signature, ulong score)
        {
            Signature = signature;
            Score = score;
        }

        public ulong Score { get; }

        public T Signature { get; }

        public override string ToString()
        {
            var type = $"Score: {Score}";
            return $"{Signature} [{type}]";
        }
    }
}