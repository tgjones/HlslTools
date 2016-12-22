using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ShaderTools.Core.Symbols;
using ShaderTools.Hlsl.Binding;

namespace ShaderTools.Hlsl.Symbols
{
    public abstract class ContainerSymbol : Symbol
    {
        private readonly List<Symbol> _members;
        private ImmutableArray<Symbol> _membersArray = ImmutableArray<Symbol>.Empty;

        internal Binder Binder { get; set; }

        public ImmutableArray<Symbol> Members
        {
            get
            {
                if (_membersArray == ImmutableArray<Symbol>.Empty)
                    _membersArray = _members.ToImmutableArray();
                return _membersArray;
            }
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
            return _members.Where(x => x is T && x.Name == name).OfType<T>();
        }

        internal void AddMember(Symbol member)
        {
            _members.Add(member);
            _membersArray = ImmutableArray<Symbol>.Empty;
        }

        internal void AddMembers(IEnumerable<Symbol> members)
        {
            _members.AddRange(members);
            _membersArray = ImmutableArray<Symbol>.Empty;
        }

        protected bool Equals(ContainerSymbol other)
        {
            return base.Equals(other) 
                && _members.Count == other._members.Count
                && _members.Zip(other._members, (x, y) => x.Equals(y)).All(x => x);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContainerSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ _members.GetHashCode();
                return hashCode;
            }
        }
    }
}