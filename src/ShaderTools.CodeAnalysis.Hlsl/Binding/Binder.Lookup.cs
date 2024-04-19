using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes;
using ShaderTools.CodeAnalysis.Hlsl.Binding.Signatures;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding
{
    internal partial class Binder
    {
        private void AddSymbol(Symbol symbol, SourceRange diagnosticSpan, bool allowDuplicates = false)
        {
            if (_symbols.Any(x => x.Key == symbol.Name && (!allowDuplicates || x.Value.Any(y => y.Kind != symbol.Kind))))
                Diagnostics.ReportSymbolRedefined(diagnosticSpan, symbol);

            List<Symbol> symbolList;
            if (!_symbols.TryGetValue(symbol.Name, out symbolList))
                _symbols.Add(symbol.Name, symbolList = new List<Symbol>());

            symbolList.Add(symbol);
        }

        protected internal virtual Dictionary<string, List<Symbol>> LocalSymbols => _symbols;

        private IEnumerable<Symbol> LookupSymbol(string name)
        {
            List<Symbol> result;
            if (LocalSymbols.TryGetValue(name, out result))
                return result;
            return Enumerable.Empty<Symbol>();
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
            var result = binder.LookupSymbol(text).Where(filter).ToList();
            if (result.Any())
                return result;

            foreach (var additionalBinder in binder.GetAdditionalParentBinders())
            {
                result = LookupSymbolRecursive(additionalBinder, text, filter).ToList();
                if (result.Any())
                    return result;
            }

            return Enumerable.Empty<Symbol>();
        }

        private IEnumerable<ContainerSymbol> LookupNamespaceOrClass(SyntaxToken name)
        {
            return LookupSymbols<NamespaceSymbol>(name)
                .Cast<ContainerSymbol>()
                .Union(LookupSymbols<StructSymbol>(name));
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

                    if (containers.Length > 1 && containers.Any(x => x is StructSymbol))
                        Diagnostics.ReportAmbiguousType(((IdentifierNameSyntax) name).Name, containers);

                    // only the Last namespace container has all members
                    Bind((IdentifierNameSyntax) name, x => new BoundName(containers.Last()));

                    return containers.Last();
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

            // the same as LookupNamespaceOrClass but for container
            var members = container.LookupMembers<NamespaceSymbol>(name.Text)
                .Cast<ContainerSymbol>()
                .Union(container.LookupMembers<StructSymbol>(name.Text))
                .ToImmutableArray();

            if (members.Length == 0)
            {
                Diagnostics.ReportUndeclaredType(name);
                return null;
            }

            if (members.Length > 1 && members.Any(x => x is StructSymbol))
                Diagnostics.ReportAmbiguousType(name, members);

            // only the Last namespace container has all members
            return members.Last();
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

                    if (containers.Length > 1 && containers.Any(x => x is StructSymbol))
                        Diagnostics.ReportAmbiguousType(((IdentifierDeclarationNameSyntax) name).Name, containers);

                    // only the Last namespace container has all members
                    Bind((IdentifierDeclarationNameSyntax) name, x => new BoundName(containers.Last()));

                    return containers.Last();
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

        private OverloadResolutionResult<FunctionSymbolSignature> LookupFunction(ContainerSymbol container, SyntaxToken name, ImmutableArray<TypeSymbol> argumentTypes)
        {
            var signatures = from f in container.LookupMembers<FunctionSymbol>(name.Text)
                             select new FunctionSymbolSignature(f);
            return OverloadResolution.Perform(signatures, argumentTypes);
        }

        private OverloadResolutionResult<FunctionSymbolSignature> LookupNumericConstructor(TypeSymbol type, ImmutableArray<TypeSymbol> argumentTypes)
        {
            var signatures = IntrinsicNumericConstructors.AllFunctions
                .Where(x => x.ReturnType.Equals(type))
                .Select(x => new FunctionSymbolSignature(x));

            var resolutionResult = OverloadResolution.Perform(signatures, argumentTypes);

            if (type.Kind == SymbolKind.IntrinsicMatrixType && resolutionResult.Selected == null)
            {
                // If no existing signatures for matrix constructor, then as long as we have the correct arguments, create a new function symbol on-the-fly.
                // This is to handle the MANY overloads of matrix constructor functions, which are too many to create and store statically.

                if (argumentTypes.All(x => x is IntrinsicNumericTypeSymbol))
                {
                    var matrixType = (IntrinsicMatrixTypeSymbol) type;

                    var totalScalars = matrixType.Rows * matrixType.Cols;

                    var numericArgumentTypes = argumentTypes
                        .Cast<IntrinsicNumericTypeSymbol>()
                        .ToImmutableArray();

                    var totalScalarsInArgs = numericArgumentTypes
                        .Sum(x => x.GetNumElements());

                    if (totalScalars == totalScalarsInArgs)
                    {
                        signatures = new[]
                        {
                            new FunctionSymbol(matrixType.Name, string.Empty, null, matrixType,
                                f => numericArgumentTypes.Select((t, i) => new ParameterSymbol($"arg{i}", string.Empty, f, t.GetNumericTypeWithScalarType(matrixType.ScalarType))).ToArray(),
                                isNumericConstructor: true)
                        }.Select(x => new FunctionSymbolSignature(x));

                        resolutionResult = OverloadResolution.Perform(signatures, argumentTypes);
                    }
                }
            }

            return resolutionResult;
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