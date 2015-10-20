using System.Collections.Immutable;

namespace HlslTools.Symbols
{
    public interface IInvocableSymbol
    {
        ImmutableArray<ParameterSymbol> Parameters { get; }
    }
}