namespace HlslTools.Symbols
{
    internal static class TypeFacts
    {
        public static readonly TypeSymbol Missing = new IntrinsicTypeSymbol("[Missing]", string.Empty);
        public static readonly TypeSymbol Unknown = new IntrinsicTypeSymbol("[Unknown]", string.Empty);

        public static bool IsMissing(this TypeSymbol type)
        {
            return type == Missing;
        }

        public static bool IsUnknown(this TypeSymbol type)
        {
            return type == Unknown;
        }

        public static bool IsError(this TypeSymbol type)
        {
            return type.IsMissing() || type.IsUnknown();
        }

        public static string ToDisplayName(this TypeSymbol type)
        {
            if (type.IsUnknown())
                return "<?>";

            if (type.IsMissing())
                return "<missing>";

            return type.Name;
        }
    }
}