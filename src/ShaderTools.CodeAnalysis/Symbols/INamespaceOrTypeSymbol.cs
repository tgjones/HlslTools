using System.Collections.Immutable;

namespace ShaderTools.CodeAnalysis.Symbols
{
    public interface INamespaceOrTypeSymbol : ISymbol
    {
        /// <summary>
        /// Get all the members of this symbol.
        /// </summary>
        /// <returns>An ImmutableArray containing all the members of this symbol. If this symbol has no members,
        /// returns an empty ImmutableArray. Never returns Null.</returns>
        ImmutableArray<ISymbol> GetMembers();
    }
}