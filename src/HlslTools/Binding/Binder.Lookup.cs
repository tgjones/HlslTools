using System;
using System.Linq;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    internal partial class Binder
    {
        private void AddSymbol(Symbol symbol)
        {
            if (_symbols.ContainsKey(symbol.Name))
                throw new InvalidOperationException();
            _symbols.Add(symbol.Name, symbol);
        }

        private TypeSymbol LookupSymbol(TypeSyntax syntax)
        {
            return syntax.GetTypeSymbol(this);
        }

        public Symbol LookupSymbol(SyntaxToken name)
        {
            Symbol result;
            if (_symbols.TryGetValue(name.Text, out result))
                return result;

            if (Parent != null)
                return Parent.LookupSymbol(name);

            // TODO: Need to return all overloads.
            return IntrinsicFunctions.AllFunctions.FirstOrDefault(x => x.Name == name.Text);
        }
    }
}