using System.Collections.Generic;
using System.Collections.Immutable;

namespace HlslTools.Symbols
{
    public abstract class TypeSymbol : Symbol
    {
        private readonly Dictionary<string, Symbol> _memberTable;
        private readonly List<Symbol> _members;
        private ImmutableArray<Symbol> _membersArray;

        public ImmutableArray<Symbol> Members
        {
            get
            {
                if (_membersArray == ImmutableArray<Symbol>.Empty)
                    _membersArray = _members.ToImmutableArray();
                return _membersArray;
            }
        }

        protected TypeSymbol(SymbolKind kind, string name, string documentation, Symbol parent)
            : base(kind, name, documentation, parent)
        {
            _members = new List<Symbol>();
            _memberTable = new Dictionary<string, Symbol>();
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
            _memberTable.TryGetValue(name, out result);
            return result;
        }

        internal void AddMember(Symbol member)
        {
            _members.Add(member);
            _memberTable[member.Name] = member;
            _membersArray = ImmutableArray<Symbol>.Empty;
        }
    }
}