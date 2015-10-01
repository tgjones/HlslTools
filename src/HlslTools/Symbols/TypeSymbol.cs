using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace HlslTools.Symbols
{
    public abstract class TypeSymbol : Symbol, ISymbolTable
    {
        private readonly Func<TypeSymbol, IEnumerable<MemberSymbol>> _lazyMembers;
        private ImmutableArray<MemberSymbol> _members;
        private Dictionary<string, MemberSymbol> _memberTable;
        private ISymbolTable _parentSymbolTable;

        public ImmutableArray<MemberSymbol> Members
        {
            get
            {
                if (_members.IsDefault)
                    _members = _lazyMembers(this).ToImmutableArray();
                return _members;
            }
        }

        private Dictionary<string, MemberSymbol> MemberTable
        {
            get
            {
                if (_memberTable == null)
                {
                    _memberTable = new Dictionary<string, MemberSymbol>();
                    foreach (var member in Members)
                        _memberTable.Add(member.Name, member);
                }
                return _memberTable;
            }
        }

        protected TypeSymbol(SymbolKind kind, string name, string documentation, Symbol parent, Func<TypeSymbol, IEnumerable<MemberSymbol>> lazyMembers)
            : base(kind, name, documentation, parent)
        {
            _lazyMembers = lazyMembers;
        }

        public string FullName
        {
            get
            {
                var result = string.Empty;
                if (Parent != null)
                    result += Parent.Name + "``";
                result += Name;
                return result;
            }
        }

        public virtual TypeSymbol GetBaseType()
        {
            return null;
        }

        internal void SetParentSymbolTable(ISymbolTable symbolTable)
        {
            Debug.Assert(_parentSymbolTable == null);
            Debug.Assert(symbolTable != null);

            _parentSymbolTable = symbolTable;
        }

        public MemberSymbol GetMember(string name)
        {
            MemberSymbol result;
            MemberTable.TryGetValue(name, out result);
            return result;
        }

        #region ISymbolTable Members

        ICollection ISymbolTable.Symbols => Members;

        Symbol ISymbolTable.FindSymbol(string name, Symbol context)
        {
            Debug.Assert(string.IsNullOrEmpty(name) == false);
            Debug.Assert(context != null);

            Symbol symbol = GetMember(name);

            if (symbol == null)
            {
                var baseType = GetBaseType();
                if (baseType != null)
                    symbol = ((ISymbolTable)baseType).FindSymbol(name, context);
            }

            if (symbol == null && _parentSymbolTable != null)
                symbol = _parentSymbolTable.FindSymbol(name, context);

            return symbol;
        }
        #endregion
    }
}