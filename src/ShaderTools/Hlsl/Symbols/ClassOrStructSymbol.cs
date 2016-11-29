using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ShaderTools.Hlsl.Symbols
{
    public abstract class ClassOrStructSymbol : TypeSymbol
    {
        public ClassOrStructSymbol BaseType { get; }
        public ImmutableArray<InterfaceSymbol> BaseInterfaces { get; }

        internal ClassOrStructSymbol(SymbolKind kind, string name, Symbol parent, ClassOrStructSymbol baseType, ImmutableArray<InterfaceSymbol> baseInterfaces)
            : base(kind, name, string.Empty, parent)
        {
            BaseType = baseType;
            BaseInterfaces = baseInterfaces;
        }

        public override IEnumerable<T> LookupMembers<T>(string name)
        {
            var result = base.LookupMembers<T>(name);

            if (BaseType != null)
                result = result.Concat(BaseType.LookupMembers<T>(name));

            foreach (var baseInterface in BaseInterfaces)
                result = result.Concat(baseInterface.LookupMembers<T>(name));

            return result;
        }

        private bool Equals(ClassOrStructSymbol other)
        {
            return base.Equals(other)
                   && Equals(BaseType, other.BaseType)
                   && BaseInterfaces.Length == other.BaseInterfaces.Length
                   && BaseInterfaces.Zip(other.BaseInterfaces, (x, y) => x.Equals(y)).All(x => x);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ClassOrStructSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (BaseType?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ BaseInterfaces.GetHashCode();
                return hashCode;
            }
        }
    }
}