namespace ShaderTools.CodeAnalysis.Symbols
{
    public interface IAliasSymbol : ISymbol
    {
        /// <summary>
        /// Gets the <see cref="ITypeSymbol"/> for the
        /// namespace or type referenced by the alias.
        /// </summary>
        ITypeSymbol Target { get; }
    }
}
