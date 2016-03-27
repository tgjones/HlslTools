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
    }
}