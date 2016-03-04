using System.Collections.Generic;

namespace HlslTools.Symbols
{
    internal sealed class SymbolScopeStack
    {
        private readonly Stack<SymbolScope> _stack;

        public SymbolScope CurrentScope => _stack.Peek();

        public SymbolScopeStack()
        {
            _stack = new Stack<SymbolScope>();
            _stack.Push(new SymbolScope(null));
        }

        public void Push()
        {
            _stack.Push(new SymbolScope(_stack.Peek()));
        }

        public void Pop()
        {
            _stack.Pop();
        }

        public void AddSymbol(Symbol symbol)
        {
            _stack.Peek().AddSymbol(symbol);
        }
    }
}