using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding.Signatures;
using HlslTools.Symbols;

namespace HlslTools.Binding
{
    internal static class OverloadResolution
    {
        public static OverloadResolutionResult<T> Perform<T>(IEnumerable<T> signatures, params TypeSymbol[] argumentTypes)
            where T : Signature
        {
            return Perform(signatures, (IReadOnlyCollection<TypeSymbol>)argumentTypes);
        }

        public static OverloadResolutionResult<T> Perform<T>(IEnumerable<T> signatures, IReadOnlyCollection<TypeSymbol> argumentTypes)
            where T : Signature
        {
            var candidates = (from s in signatures
                              let conversions = argumentTypes.Zip(s.GetParameterTypes(), Conversion.Classify).ToImmutableArray()
                              select new OverloadResolutionCandidate<T>(s, conversions)).ToArray();

            MarkApplicableCandidates(candidates, argumentTypes.Count);
            MarkCandidatesWithBetterAlternative(candidates, argumentTypes.Count);

            OverloadResolutionCandidate<T> best;
            OverloadResolutionCandidate<T> selected;
            GetBestAndSelectedCandidate(candidates, out best, out selected);

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

        private static void MarkCandidatesWithBetterAlternative<T>(IList<OverloadResolutionCandidate<T>> candidates, int argumentCount)
            where T : Signature
        {
            for (var i = 0; i < candidates.Count; i++)
            {
                var x = candidates[i];
                if (!x.IsApplicable)
                    continue;

                for (var j = 0; j < candidates.Count; j++)
                {
                    var y = candidates[j];
                    if (!y.IsApplicable || i == j)
                        continue;

                    var r = Compare(x, y, argumentCount);
                    if (r < 0)
                    {
                        // x is better
                        candidates[j] = candidates[j].MarkHasBetterAlternative();
                    }
                    else if (r > 0)
                    {
                        // y is better
                        candidates[i] = candidates[i].MarkHasBetterAlternative();
                    }
                }
            }
        }

        private static int Compare<T>(OverloadResolutionCandidate<T> x, OverloadResolutionCandidate<T> y, int argumentCount)
            where T : Signature
        {
            var betterConversionsInX = 0;
            var betterConversionsInY = 0;

            for (var i = 0; i < argumentCount; i++)
            {
                var cX = x.ArgumentConversions[i];
                var tX = x.Signature.GetParameterType(i);
                var cY = y.ArgumentConversions[i];
                var tY = y.Signature.GetParameterType(i);

                var comparison = Conversion.Compare(tX, cX, tY, cY);

                if (comparison < 0)
                    betterConversionsInX++;
                else if (comparison > 0)
                    betterConversionsInY++;
            }

            // NOTE: The roles are reversed here.
            return -betterConversionsInX.CompareTo(betterConversionsInY);
        }

        private static void GetBestAndSelectedCandidate<T>(IEnumerable<OverloadResolutionCandidate<T>> candidates, out OverloadResolutionCandidate<T> best, out OverloadResolutionCandidate<T> selected)
            where T : Signature
        {
            best = null;
            selected = null;

            foreach (var candidate in candidates.Where(c => c.IsSuitable))
            {
                if (best == null)
                {
                    best = selected = candidate;
                }
                else
                {
                    best = null;
                    return;
                }
            }
        }
    }
}