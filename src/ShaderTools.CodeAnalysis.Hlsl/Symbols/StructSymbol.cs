using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Binding;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class StructSymbol : TypeSymbol, INamedTypeSymbol
    {
        public StructSymbol BaseType { get; }
        public ImmutableArray<InterfaceSymbol> BaseInterfaces { get; }

        public StructTypeSyntax Syntax { get; }

        public bool IsAnonymous { get; }

        public override SyntaxTreeBase SourceTree => Syntax.SyntaxTree;
        public override ImmutableArray<SourceRange> Locations { get; }
        public override ImmutableArray<SyntaxNodeBase> DeclaringSyntaxNodes { get; }

        internal StructSymbol(StructTypeSyntax syntax, Symbol parent, StructSymbol baseType, ImmutableArray<InterfaceSymbol> baseInterfaces, Binder binder)
            : base(syntax.IsClass ? SymbolKind.Class : SymbolKind.Struct, (syntax.Name != null) ? syntax.Name.Text : "<anonymous struct>", string.Empty, parent)
        {
            Syntax = syntax;
            BaseType = baseType;
            BaseInterfaces = baseInterfaces;
            Binder = binder;

            IsAnonymous = syntax.Name == null;

            Locations = syntax.Name != null
                ? ImmutableArray.Create(syntax.Name.SourceRange)
                : ImmutableArray<SourceRange>.Empty;

            DeclaringSyntaxNodes = ImmutableArray.Create((SyntaxNodeBase) syntax);
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

        private bool Equals(StructSymbol other)
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
            return obj.GetType() == GetType() && Equals((StructSymbol)obj);
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