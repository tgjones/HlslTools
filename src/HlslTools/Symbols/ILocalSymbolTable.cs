namespace HlslTools.Symbols
{
    internal interface ILocalSymbolTable : ISymbolTable
    {
        void AddSymbol(LocalSymbol symbol);

        void PushScope();
        void PopScope();
    }
}