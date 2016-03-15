using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public abstract class TypeSymbol : Symbol
    {
        private readonly Dictionary<string, Symbol> _memberTable;

        // TODO: Should be read-only.
        public List<Symbol> Members { get; }

        protected TypeSymbol(SymbolKind kind, string name, string documentation, Symbol parent)
            : base(kind, name, documentation, parent)
        {
            Members = new List<Symbol>();
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
            Members.Add(member);
            _memberTable[member.Name] = member;
        }
    }
}