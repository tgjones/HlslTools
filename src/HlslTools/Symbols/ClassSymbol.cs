using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class ClassSymbol : TypeSymbol
    {
        public ClassSymbol BaseClass { get; }
        public ImmutableArray<InterfaceSymbol> BaseInterfaces { get; }

        public ClassSymbol(ClassTypeSyntax syntax, Symbol parent, ClassSymbol baseClass, ImmutableArray<InterfaceSymbol> baseInterfaces)
            : base(SymbolKind.Class, syntax.Name.Text, string.Empty, parent)
        {
            BaseClass = baseClass;
            BaseInterfaces = baseInterfaces;
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