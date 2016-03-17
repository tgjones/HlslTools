using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding.Signatures;
using HlslTools.Symbols;

namespace HlslTools.Binding
{
    internal static class OverloadResolution
    {
        public static OverloadResolutionResult<T> Perform<T>(IEnumerable<T> signatures, IReadOnlyCollection<TypeSymbol> argumentTypes)
            where T : Signature
        {
            var candidates = (from s in signatures
                              let conversions = argumentTypes.Zip(s.GetParameterTypes(), Conversion.Classify).ToImmutableArray()
                              select new OverloadResolutionCandidate<T>(s, argumentTypes, conversions)).ToArray();

            MarkApplicableCandidates(candidates, argumentTypes.Count);
            MarkCandidatesScore(candidates, argumentTypes.Count);

            OverloadResolutionCandidate<T> best;
            OverloadResolutionCandidate<T> selected;

            candidates = candidates.Where(x => x.IsApplicable).OrderBy(x => x.Score).ToArray();
            best = selected = candidates.FirstOrDefault();
            // TODO: Don't need "selected" anymore.

            return new OverloadResolutionResult<T>(best, selected, candidates);
        }

        private static void MarkApplicableCandidates<T>(IList<OverloadResolutionCandidate<T>> candidates, int argumentCount)
            where T : Signature
        {
            for (var i = 0; i < candidates.Count; i++)
            {
                if (!IsApplicable(candidates[i], argumentCount))
                    continue;

                candidates[i] = candidates[i].MarkApplicable();
            }
        }

        private static bool IsApplicable<T>(OverloadResolutionCandidate<T> candidate, int argumentCount)
            where T : Signature
        {
            return (candidate.Signature.ParameterCount == argumentCount && candidate.ArgumentConversions.All(c => c.IsImplicit))
                || (candidate.Signature.HasVariadicParameter && argumentCount >= candidate.Signature.ParameterCount); // TODO: Need to check that non-variadic argument / parameter types match.
        }

        private static void MarkCandidatesScore<T>(IList<OverloadResolutionCandidate<T>> candidates, int argumentCount)
            where T : Signature
        {
            for (var i = 0; i < candidates.Count; i++)
            {
                var x = candidates[i];
                if (!x.IsApplicable)
                    continue;

                var score = GetScore(x, argumentCount);
                candidates[i] = candidates[i].MarkScore(score);
            }
        }

        private static int GetScore<T>(OverloadResolutionCandidate<T> x, int argumentCount)
            where T : Signature
        {
            var score = 0;

            for (var i = 0; i < argumentCount; i++)
            {
                var cX = x.ArgumentConversions[i];
                var tX = x.Signature.GetParameterType(i);

                score += Conversion.GetScore(x.ArgumentTypes[i], tX, cX);
            }

            return score;
        }
    }
}