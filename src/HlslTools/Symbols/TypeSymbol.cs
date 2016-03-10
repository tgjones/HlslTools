using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace HlslTools.Symbols
{
    public abstract class TypeSymbol : Symbol
    {
        private readonly Func<TypeSymbol, IEnumerable<Symbol>> _lazyMembers;
        private ImmutableArray<Symbol> _members;
        private Dictionary<string, Symbol> _memberTable;

        public ImmutableArray<Symbol> Members
        {
            get
            {
                if (_members.IsDefault)
                    _members = _lazyMembers(this).ToImmutableArray();
                return _members;
            }
        }

        private Dictionary<string, Symbol> MemberTable
        {
            get
            {
                if (_memberTable == null)
                {
                    _memberTable = new Dictionary<string, Symbol>();
                    foreach (var member in Members)
                        _memberTable.Add(member.Name, member);
                }
                return _memberTable;
            }
        }

        protected TypeSymbol(SymbolKind kind, string name, string documentation, Symbol parent, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers)
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

        public Symbol GetMember(string name)
        {
            Symbol result;
            MemberTable.TryGetValue(name, out result);
            return result;
        }
    }
}