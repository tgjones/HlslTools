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

        protected virtual IEnumerable<Symbol> LocalSymbols => _symbols;

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

        private TypeSymbol LookupType(TypeSyntax syntax)
        {
            var type = syntax.GetTypeSymbol(this);
            if (type != null)
                return type;

            Diagnostics.ReportUndeclaredType(syntax);
            return TypeFacts.Unknown;
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

        private IEnumerable<Symbol> LookupSymbols(SyntaxToken name, Func<Symbol, bool> filter)
        {
            var text = name.ValueText;

            IEnumerable<Symbol> result;
            var binder = this;
            do
            {
                result = binder.LookupSymbol(text).Where(filter);
                binder = binder.Parent;
            } while (!result.Any() && binder != null);

            return result;
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