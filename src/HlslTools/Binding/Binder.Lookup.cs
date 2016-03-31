using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding.BoundNodes;
using HlslTools.Binding.Signatures;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Syntax;
using HlslTools.Text;

namespace HlslTools.Binding
{
    internal partial class Binder
    {
        private void AddSymbol(Symbol symbol, TextSpan diagnosticSpan, bool allowDuplicates = false)
        {
            if (_symbols.Any(x => x.Name == symbol.Name && (!allowDuplicates || x.Kind != symbol.Kind)))
                Diagnostics.ReportSymbolRedefined(diagnosticSpan, symbol);

            _symbols.Add(symbol);
        }

        protected internal virtual IEnumerable<Symbol> LocalSymbols => _symbols;

        private IEnumerable<Symbol> LookupSymbol(string name)
        {
            return LocalSymbols.Where(x => x.Name == name);
        }

        private NamespaceSymbol LookupEnclosingNamespace()
        {
            var binder = this;
            while (binder != null)
            {
                var namespaceBinder = binder as NamespaceBinder;
                if (namespaceBinder != null)
                    return namespaceBinder.NamespaceSymbol;
                binder = binder.Parent;
            }

            return null;
        }

        private IEnumerable<T> LookupSymbols<T>(SyntaxToken name)
            where T : Symbol
        {
            return LookupSymbols(name, s => s is T).OfType<T>();
        }

        private IEnumerable<VariableSymbol> LookupVariable(SyntaxToken name)
        {
            return LookupSymbols<VariableSymbol>(name);
        }

        private IEnumerable<FieldSymbol> LookupField(TypeSymbol type, SyntaxToken name)
        {
            return type.LookupMembers<FieldSymbol>(name.Text);
        }

        public IEnumerable<TypeSymbol> LookupTypeSymbol(SyntaxToken name)
        {
            return LookupSymbols<TypeSymbol>(name);
        }

        public TypeSymbol LookupTypeSymbol(StructTypeSyntax structType)
        {
            return LocalSymbols.OfType<StructSymbol>().FirstOrDefault(x => x.Syntax == structType);
        }

        protected virtual IEnumerable<Binder> GetAdditionalParentBinders()
        {
            yield break;
        }

        protected internal IEnumerable<Binder> GetBinderChain()
        {
            var parent = this;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }

        private IEnumerable<Symbol> LookupSymbols(SyntaxToken name, Func<Symbol, bool> filter)
        {
            var text = name.ValueText;

            IEnumerable<Symbol> result = Enumerable.Empty<Symbol>();
            foreach (var binder in GetBinderChain())
            {
                result = LookupSymbolRecursive(binder, text, filter);
                if (result.Any())
                    break;
            }

            return result;
        }

        private static IEnumerable<Symbol> LookupSymbolRecursive(Binder binder, string text, Func<Symbol, bool> filter)
        {
            var result = binder.LookupSymbol(text).Where(filter);
            if (result.Any())
                return result;

            foreach (var additionalBinder in binder.GetAdditionalParentBinders())
            {
                result = LookupSymbolRecursive(additionalBinder, text, filter);
                if (result.Any())
                    return result;
            }

            return Enumerable.Empty<Symbol>();
        }

        private IEnumerable<ContainerSymbol> LookupNamespaceOrClass(SyntaxToken name)
        {
            return LookupSymbols<NamespaceSymbol>(name)
                .Cast<ContainerSymbol>()
                .Union(LookupSymbols<ClassSymbol>(name));
        }

        private ContainerSymbol LookupContainer(NameSyntax name)
        {
            switch (name.Kind)
            {
                case SyntaxKind.IdentifierName:
                    var containers = LookupNamespaceOrClass(((IdentifierNameSyntax) name).Name).ToImmutableArray();

                    if (containers.Length == 0)
                    {
                        Diagnostics.ReportUndeclaredType(name);
                        return null;
                    }

                    if (containers.Length > 1)
                        Diagnostics.ReportAmbiguousType(((IdentifierNameSyntax) name).Name, containers);

                    Bind((IdentifierNameSyntax) name, x => new BoundName(containers.First()));

                    return containers.First();
                case SyntaxKind.QualifiedName:
                    var qualifiedName = (QualifiedNameSyntax) name;
                    var leftContainer = LookupContainer(qualifiedName.Left);
                    var result = LookupContainerMember(leftContainer, qualifiedName.Right.Name);
                    Bind(qualifiedName.Right, x => new BoundName(result));
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }

        private ContainerSymbol LookupContainerMember(ContainerSymbol container, SyntaxToken name)
        {
            if (container == null)
                return null;

            var members = container.LookupMembers<NamespaceSymbol>(name.Text)
                .Cast<ContainerSymbol>()
                .Union(container.LookupMembers<ClassSymbol>(name.Text))
                .ToImmutableArray();

            if (members.Length == 0)
            {
                Diagnostics.ReportUndeclaredType(name);
                return null;
            }

            if (members.Length > 1)
                Diagnostics.ReportAmbiguousType(name, members);

            return members.First();
        }

        private ContainerSymbol LookupContainer(DeclarationNameSyntax name)
        {
            switch (name.Kind)
            {
                case SyntaxKind.IdentifierDeclarationName:
                    var containers = LookupNamespaceOrClass(((IdentifierDeclarationNameSyntax) name).Name).ToImmutableArray();

                    if (containers.Length == 0)
                    {
                        Diagnostics.ReportUndeclaredType(name);
                        return null;
                    }

                    if (containers.Length > 1)
                        Diagnostics.ReportAmbiguousType(((IdentifierDeclarationNameSyntax) name).Name, containers);

                    Bind((IdentifierDeclarationNameSyntax) name, x => new BoundName(containers.First()));

                    return containers.First();
                case SyntaxKind.QualifiedDeclarationName:
                    var qualifiedName = (QualifiedDeclarationNameSyntax) name;
                    var leftContainer = LookupContainer(qualifiedName.Left);
                    var result = LookupContainerMember(leftContainer, qualifiedName.Right.Name);
                    Bind(qualifiedName.Right, x => new BoundName(result));
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }

        private IEnumerable<IndexerSymbol> LookupIndexer(TypeSymbol type)
        {
            return type.LookupMembers<IndexerSymbol>("[]");
        }

        private OverloadResolutionResult<IndexerSymbolSignature> LookupIndexer(TypeSymbol type, ImmutableArray<TypeSymbol> argumentTypes)
        {
            var signatures = from m in LookupIndexer(type)
                             select new IndexerSymbolSignature(m);
            return OverloadResolution.Perform(signatures, argumentTypes);
        }

        private IEnumerable<FunctionSymbol> LookupMethod(TypeSymbol type, SyntaxToken name)
        {
            return type.LookupMembers<FunctionSymbol>(name.Text);
        }

        private OverloadResolutionResult<FunctionSymbolSignature> LookupMethod(TypeSymbol type, SyntaxToken name, ImmutableArray<TypeSymbol> argumentTypes)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            var signatures = from m in LookupMethod(type, name)
                             select new FunctionSymbolSignature(m);
            return OverloadResolution.Perform(signatures, argumentTypes);
        }

        private OverloadResolutionResult<FunctionSymbolSignature> LookupFunction(SyntaxToken name, ImmutableArray<TypeSymbol> argumentTypes)
        {
            var signatures = from f in LookupSymbols<FunctionSymbol>(name)
                             where name.Text == f.Name
                             select new FunctionSymbolSignature(f);
            return OverloadResolution.Perform(signatures, argumentTypes);
        }

        private static OverloadResolutionResult<BinaryOperatorSignature> LookupBinaryOperator(BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right)
        {
            return BinaryOperator.Resolve(operatorKind, left.Type, right.Type);
        }

        private static OverloadResolutionResult<UnaryOperatorSignature> LookupUnaryOperator(UnaryOperatorKind operatorKind, BoundExpression operand)
        {
            return UnaryOperator.Resolve(operatorKind, operand.Type);
        }
    }
}