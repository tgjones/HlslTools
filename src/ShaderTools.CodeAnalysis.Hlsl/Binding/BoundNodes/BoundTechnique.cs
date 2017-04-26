using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundTechnique : BoundNode
    {
        public TechniqueSymbol TechniqueSymbol { get; }
        public ImmutableArray<BoundPass> Passes { get; }

        public BoundTechnique(TechniqueSymbol techniqueSymbol, ImmutableArray<BoundPass> passes)
            : base(BoundNodeKind.Technique)
        {
            TechniqueSymbol = techniqueSymbol;
            Passes = passes;
        }
    }
}