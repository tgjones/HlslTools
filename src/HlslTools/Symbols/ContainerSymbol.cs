using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding;

namespace HlslTools.Symbols
{
    public abstract class ContainerSymbol : Symbol
    {
        private readonly List<Symbol> _members;
        private ImmutableArray<Symbol> _membersArray = ImmutableArray<Symbol>.Empty;
        private bool _lazyMembersComputed;

        internal Binder Binder { get; set; }

        public ImmutableArray<Symbol> Members
        {
            get
            {
                if (_membersArray == ImmutableArray<Symbol>.Empty)
                {
                    EnsureLazyMembers();
                    _membersArray = _members.ToImmutableArray();
                }
                return _membersArray;
            }
        }

        private void EnsureLazyMembers()
        {
            if (!_lazyMembersComputed)
            {
                ComputeLazyMembers();
                _lazyMembersComputed = true;
            }
        }

        protected virtual void ComputeLazyMembers()
        {
        }

        internal ContainerSymbol(SymbolKind kind, string name, string documentation, Symbol parent)
            : base(kind, name, documentation, parent)
        {
            _members = new List<Symbol>();
        }

        public string FullName
        {
            get
            {
                var result = string.Empty;
                if (Parent != null)
                    result += Parent.Name + "::";
                result += Name;
                return result;
            }
        }

        public virtual IEnumerable<T> LookupMembers<T>(string name)
            where T : Symbol
        {
            EnsureLazyMembers();
            return _members.Where(x => x is T && x.Name == name).OfType<T>();
        }

        internal void AddMember(Symbol member)
        {
            _members.Add(member);
            _membersArray = ImmutableArray<Symbol>.Empty;
        }
    }
}