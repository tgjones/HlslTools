namespace HlslTools.Compilation
{
    public sealed class Conversion
    {
        private Conversion(bool exists, bool isIdentity, bool isImplicitWidening, bool isImplicitNarrowing)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicitWidening = isImplicitWidening;
            IsImplicitNarrowing = isImplicitNarrowing;
        }

        // TODO: Have separate properties for type widening / narrowing and dimension widening / narrowing

        internal static readonly Conversion None = new Conversion(false, false, false, false);
        internal static readonly Conversion Identity = new Conversion(true, true, true, true);
        internal static readonly Conversion ImplicitWidening = new Conversion(true, false, true, false);
        internal static readonly Conversion ImplicitNarrowing = new Conversion(true, false, false, true);
        internal static readonly Conversion Explicit = new Conversion(true, false, false, false);

        public bool Exists { get; }

        public bool IsIdentity { get; }

        public bool IsImplicit => IsImplicitWidening || IsImplicitNarrowing;
        public bool IsImplicitWidening { get; }
        public bool IsImplicitNarrowing { get; }

        public bool IsExplicit => Exists && !IsImplicit;
    }
}