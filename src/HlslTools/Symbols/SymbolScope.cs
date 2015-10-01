using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace HlslTools.Symbols
{
    internal sealed class SymbolScope : ISymbolTable
    {
        private readonly ISymbolTable _parentSymbolTable;
        private List<SymbolScope> _childScopes;

        private readonly Dictionary<string, LocalSymbol> _localTable;
        private readonly Collection<LocalSymbol> _locals;

        public SymbolScope(SymbolScope parentScope)
            : this((ISymbolTable)parentScope)
        {
            Parent = parentScope;
        }

        public SymbolScope(ISymbolTable parentSymbolTable)
        {
            Debug.Assert(parentSymbolTable != null);
            _parentSymbolTable = parentSymbolTable;

            _locals = new Collection<LocalSymbol>();
            _localTable = new Dictionary<string, LocalSymbol>();
        }

        public ICollection<SymbolScope> ChildScopes => _childScopes;

        public SymbolScope Parent { get; }

        public void AddChildScope(SymbolScope scope)
        {
            if (_childScopes == null)
                _childScopes = new List<SymbolScope>();
            _childScopes.Add(scope);
        }

        public void AddSymbol(LocalSymbol symbol)
        {
            Debug.Assert(symbol != null);
            Debug.Assert(String.IsNullOrEmpty(symbol.Name) == false);
            Debug.Assert(_localTable.ContainsKey(symbol.Name) == false);

            _locals.Add(symbol);
            _localTable[symbol.Name] = symbol;
        }

        #region ISymbolTable Members

        ICollection ISymbolTable.Symbols => _locals;

        Symbol ISymbolTable.FindSymbol(string name, Symbol context)
        {
            Symbol symbol = null;

            if (_localTable.ContainsKey(name))
            {
                symbol = _localTable[name];
            }

            if (symbol == null)
            {
                Debug.Assert(_parentSymbolTable != null);
                symbol = _parentSymbolTable.FindSymbol(name, context);
            }

            return symbol;
        }
        #endregion
    }
}