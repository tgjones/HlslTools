using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class IntrinsicObjectTypeSymbol : IntrinsicTypeSymbol
    {
        public PredefinedObjectType PredefinedType { get; }

        internal IntrinsicObjectTypeSymbol(string name, string documentation, PredefinedObjectType predefinedType)
            : base(SymbolKind.IntrinsicObjectType, name, documentation)
        {
            PredefinedType = predefinedType;
        }
    }
}