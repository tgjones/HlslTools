using System.Collections.Immutable;

namespace HlslTools.Symbols
{
    public sealed class SemanticSymbol : Symbol
    {
        internal SemanticSymbol(string name, string documentation, bool allowsMultiple, SemanticUsages usages, params TypeSymbol[] valueTypes)
            : base(SymbolKind.Semantic, name, documentation, null)
        {
            AllowsMultiple = allowsMultiple;
            Usages = usages;
            ValueTypes = valueTypes.ToImmutableArray();
        }

        public bool AllowsMultiple { get; }
        public SemanticUsages Usages { get; }
        public ImmutableArray<TypeSymbol> ValueTypes { get; }

        public string FullDescription
        {
            get
            {
                var usageString = string.Empty;
                if (Usages.HasFlag(SemanticUsages.VertexShaderInput))
                    usageString += "\n- Vertex shader input";
                if (Usages.HasFlag(SemanticUsages.VertexShaderOutput))
                    usageString += "\n- Vertex shader output";
                if (Usages.HasFlag(SemanticUsages.HullShaderInput))
                    usageString += "\n- Hull shader input";
                if (Usages.HasFlag(SemanticUsages.HullShaderOutput))
                    usageString += "\n- Hull shader output";
                if (Usages.HasFlag(SemanticUsages.DomainShaderInput))
                    usageString += "\n- Domain shader input";
                if (Usages.HasFlag(SemanticUsages.DomainShaderOutput))
                    usageString += "\n- Domain shader output";
                if (Usages.HasFlag(SemanticUsages.GeometryShaderInput))
                    usageString += "\n- Geometry shader input";
                if (Usages.HasFlag(SemanticUsages.GeometryShaderOutput))
                    usageString += "\n- Geometry shader output";
                if (Usages.HasFlag(SemanticUsages.PixelShaderInput))
                    usageString += "\n- Pixel shader input";
                if (Usages.HasFlag(SemanticUsages.PixelShaderOutput))
                    usageString += "\n- Pixel shader output";
                return $"{Documentation}\n\nAvailable for use in:{usageString}";
            }
        }
    }
}