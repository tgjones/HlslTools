using System.Collections;

namespace HlslTools.Symbols
{
    internal interface ISymbolTable
    {
        ICollection Symbols { get; }

        Symbol FindSymbol(string name, Symbol context);
    }
}