using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class ClassSymbol : TypeSymbol
    {
        public ClassTypeSyntax Syntax { get; }
        public ClassSymbol BaseClass { get; }
        public ImmutableArray<InterfaceSymbol> BaseInterfaces { get; }

        internal ClassSymbol(ClassTypeSyntax syntax, Symbol parent, ClassSymbol baseClass, ImmutableArray<InterfaceSymbol> baseInterfaces, Binder binder)
            : base(SymbolKind.Class, syntax.Name.Text, string.Empty, parent)
        {
            Syntax = syntax;
            BaseClass = baseClass;
            BaseInterfaces = baseInterfaces;
            Binder = binder;
        }

        public override IEnumerable<T> LookupMembers<T>(string name)
        {
            var result = base.LookupMembers<T>(name);

            if (BaseClass != null)
                result = result.Concat(BaseClass.LookupMembers<T>(name));

            foreach (var baseInterface in BaseInterfaces)
                result = result.Concat(baseInterface.LookupMembers<T>(name));

            return result;
        }

        private bool Equals(ClassSymbol other)
        {
            return base.Equals(other) 
                && Equals(BaseClass, other.BaseClass)
                && BaseInterfaces.Length == other.BaseInterfaces.Length
                && BaseInterfaces.Zip(other.BaseInterfaces, (x, y) => x.Equals(y)).All(x => x);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ClassSymbol && Equals((ClassSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (BaseClass?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ BaseInterfaces.GetHashCode();
                return hashCode;
            }
        }
    }
}