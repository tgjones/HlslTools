using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding
{
    internal partial class Binder
    {
        private BoundCompilationUnit BindCompilationUnit(CompilationUnitSyntax compilationUnit)
        {
            return new BoundCompilationUnit(BindTopLevelDeclarations(compilationUnit.Declarations, null));
        }

        private ImmutableArray<BoundNode> BindTopLevelDeclarations(List<SyntaxNode> declarations, Symbol parent)
        {
            return declarations.Select(x => Bind(x, y => BindGlobalDeclaration(y, parent))).ToImmutableArray();
        }

        private BoundNode BindGlobalDeclaration(SyntaxNode declaration, Symbol parent)
        {
            switch (declaration.Kind)
            {
                case SyntaxKind.VariableDeclarationStatement:
                    return BindVariableDeclarationStatement((VariableDeclarationStatementSyntax) declaration, parent);
                case SyntaxKind.FunctionDeclaration:
                    return BindFunctionDeclaration((FunctionDeclarationSyntax) declaration, parent);
                case SyntaxKind.FunctionDefinition:
                    return BindFunctionDefinition((FunctionDefinitionSyntax) declaration, parent);
                case SyntaxKind.ConstantBufferDeclaration:
                    return BindConstantBufferDeclaration((ConstantBufferSyntax) declaration);
                case SyntaxKind.TypeDeclarationStatement:
                    return BindTypeDeclaration((TypeDeclarationStatementSyntax) declaration, parent);
                case SyntaxKind.Namespace:
                    return BindNamespace((NamespaceSyntax) declaration);
                case SyntaxKind.TechniqueDeclaration:
                    return BindTechniqueDeclaration((TechniqueSyntax) declaration);
                case SyntaxKind.TypedefStatement:
                    return BindTypedefStatement((TypedefStatementSyntax) declaration);
                case SyntaxKind.EmptyStatement:
                    return BindEmptyStatement((EmptyStatementSyntax) declaration);
                default:
                    throw new ArgumentOutOfRangeException(declaration.Kind.ToString());
            }
        }

        private BoundNode BindTechniqueDeclaration(TechniqueSyntax declaration)
        {
            var techniqueSymbol = new TechniqueSymbol(declaration);
            if (techniqueSymbol.Name != null)
                AddSymbol(techniqueSymbol, declaration.Name.SourceRange);

            var techniqueBinder = new Binder(_sharedBinderState, this);
            var boundPasses = declaration.Passes.Select(x => techniqueBinder.Bind(x, techniqueBinder.BindPass));
            return new BoundTechnique(techniqueSymbol, boundPasses.ToImmutableArray());
        }

        private BoundPass BindPass(PassSyntax syntax)
        {
            return new BoundPass();
        }

        private BoundNode BindTypedefStatement(TypedefStatementSyntax declaration)
        {
            var boundType = Bind(declaration.Type, x => BindType(x, null));

            var boundDeclarations = new List<BoundTypeAlias>();
            foreach (var declarator in declaration.Declarators)
            {
                boundDeclarations.Add(Bind(declarator, x => BindTypeAlias(x, boundType.TypeSymbol)));
            }

            return new BoundTypedefStatement(boundDeclarations.ToImmutableArray());
        }

        private BoundTypeAlias BindTypeAlias(TypeAliasSyntax syntax, TypeSymbol variableType)
        {
            variableType = BindArrayRankSpecifiers(syntax.ArrayRankSpecifiers, variableType);

            var symbol = new TypeAliasSymbol(syntax, variableType);
            AddSymbol(symbol, syntax.Identifier.SourceRange);

            var boundQualifiers = new List<BoundVariableQualifier>();
            foreach (var qualifier in syntax.Qualifiers)
                boundQualifiers.Add(Bind(qualifier, BindVariableQualifier));

            return new BoundTypeAlias(symbol, variableType, boundQualifiers.ToImmutableArray());
        }

        private BoundNode BindNamespace(NamespaceSyntax declaration)
        {
            var enclosingNamespace = LookupEnclosingNamespace();
            var namespaceSymbol = new NamespaceSymbol(declaration, enclosingNamespace);

            AddSymbol(namespaceSymbol, declaration.Name.SourceRange);

            var namespaceBinder = new NamespaceBinder(_sharedBinderState, this, namespaceSymbol);
            namespaceSymbol.Binder = namespaceBinder;

            var boundDeclarations = namespaceBinder.BindTopLevelDeclarations(declaration.Declarations, namespaceSymbol);

            foreach (var member in namespaceBinder.LocalSymbols.Values.SelectMany(x => x))
                namespaceSymbol.AddMember(member);

            return new BoundNamespace(namespaceSymbol, boundDeclarations);
        }

        private BoundMultipleVariableDeclarations BindVariableDeclaration(VariableDeclarationSyntax syntax, Symbol parent)
        {
            return BindVariableDeclaration(syntax, parent, (d, t) => new SourceVariableSymbol(d, parent, t));
        }

        private BoundMultipleVariableDeclarations BindVariableDeclaration(VariableDeclarationSyntax syntax, Symbol parent, Func<VariableDeclaratorSyntax, TypeSymbol, VariableSymbol> createSymbol)
        {
            var boundType = Bind(syntax.Type, x => BindType(x, parent));

            var boundDeclarations = new List<BoundVariableDeclaration>();
            foreach (var declarator in syntax.Variables)
            {
                boundDeclarations.Add(Bind(declarator, x => BindVariableDeclarator(x, boundType.TypeSymbol, createSymbol)));
            }

            return new BoundMultipleVariableDeclarations(boundDeclarations.ToImmutableArray());
        }

        private BoundVariableDeclaration BindVariableDeclarator(VariableDeclaratorSyntax syntax, TypeSymbol variableType, Func<VariableDeclaratorSyntax, TypeSymbol, VariableSymbol> createSymbol)
        {
            variableType = BindArrayRankSpecifiers(syntax.ArrayRankSpecifiers, variableType);

            var symbol = createSymbol(syntax, variableType);
            AddSymbol(symbol, syntax.Identifier.SourceRange);

            var boundQualifiers = new List<BoundVariableQualifier>();
            foreach (var qualifier in syntax.Qualifiers)
                boundQualifiers.Add(Bind(qualifier, BindVariableQualifier));

            BoundInitializer initializer = null;
            if (syntax.Initializer != null)
                initializer = BindInitializer(syntax.Initializer);

            return new BoundVariableDeclaration(symbol, variableType, boundQualifiers.ToImmutableArray(), initializer);
        }

        private BoundVariableQualifier BindVariableQualifier(VariableDeclaratorQualifierSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.SemanticName:
                    var semanticSyntax = (SemanticSyntax) syntax;
                    var semanticSymbol = IntrinsicSemantics.AllSemantics.FirstOrDefault(x => string.Equals(x.Name, semanticSyntax.Semantic.Text, StringComparison.OrdinalIgnoreCase))
                        ?? new SemanticSymbol(semanticSyntax.Semantic.Text, string.Empty, false, SemanticUsages.AllShaders);
                    return new BoundSemantic(semanticSymbol);
                case SyntaxKind.RegisterLocation:
                    return new BoundRegisterLocation();
                case SyntaxKind.PackOffsetLocation:
                    return new BoundPackOffsetLocation();
                default:
                    throw new ArgumentOutOfRangeException(syntax.Kind.ToString());
            }
        }

        private TypeSymbol BindArrayRankSpecifiers(List<ArrayRankSpecifierSyntax> arrayRankSpecifiers, TypeSymbol variableType)
        {
            foreach (var arrayRankSpecifier in arrayRankSpecifiers)
            {
                int? dimension = null;
                if (arrayRankSpecifier.Dimension != null)
                {
                    var boundRankSpecifier = Bind(arrayRankSpecifier.Dimension, BindExpression);
                    if (boundRankSpecifier.Kind == BoundNodeKind.LiteralExpression)
                        dimension = Convert.ToInt32(((BoundLiteralExpression) boundRankSpecifier).Value);
                }
                variableType = new ArraySymbol(variableType, dimension);
            }
            return variableType;
        }

        private BoundFunctionDeclaration BindFunctionDeclaration(FunctionDeclarationSyntax declaration, Symbol parent)
        {
            BindAttributes(declaration.Attributes);

            var boundReturnType = Bind(declaration.ReturnType, x => BindType(x, parent));

            var functionSymbol = LocalSymbols.OfType<SourceFunctionSymbol>()
                .FirstOrDefault(x => SyntaxFacts.HaveMatchingSignatures(
                    x.DefinitionSyntax as FunctionSyntax ?? x.DeclarationSyntaxes[0],
                    declaration));

            if (functionSymbol != null)
            {
                functionSymbol.DeclarationSyntaxes.Add(declaration);
            }
            else
            {
                functionSymbol = new SourceFunctionSymbol(declaration, parent, boundReturnType.TypeSymbol);
                AddSymbol(functionSymbol, declaration.Name.SourceRange, true);
            }

            if (declaration.Semantic != null)
                Bind(declaration.Semantic, BindVariableQualifier);

            var functionBinder = new FunctionBinder(_sharedBinderState, this, functionSymbol);

            var boundParameters = BindParameters(declaration.ParameterList, functionBinder, functionSymbol);

            return new BoundFunctionDeclaration(functionSymbol, boundReturnType, boundParameters.ToImmutableArray());
        }

        private BoundNode BindFunctionDefinition(FunctionDefinitionSyntax declaration, Symbol parent)
        {
            BindAttributes(declaration.Attributes);

            var boundReturnType = Bind(declaration.ReturnType, x => BindType(x, parent));

            var isQualifiedName = false;

            ContainerSymbol containerSymbol;
            Symbol functionOwner;
            switch (declaration.Name.Kind)
            {
                case SyntaxKind.IdentifierDeclarationName:
                    containerSymbol = null;
                    functionOwner = parent;
                    break;
                case SyntaxKind.QualifiedDeclarationName:
                    containerSymbol = LookupContainer(((QualifiedDeclarationNameSyntax) declaration.Name).Left);
                    if (containerSymbol == null)
                        return new BoundErrorNode();
                    isQualifiedName = true;
                    functionOwner = containerSymbol;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var containerBinder = containerSymbol?.Binder ?? this;

            var symbolTable = containerBinder.LocalSymbols;

            var functionSymbol = symbolTable
                .SelectMany(x => x.Value)
                .OfType<SourceFunctionSymbol>()
                .FirstOrDefault(x => SyntaxFacts.HaveMatchingSignatures(
                    x.DefinitionSyntax as FunctionSyntax ?? x.DeclarationSyntaxes[0],
                    declaration));

            if (functionSymbol != null)
            {
                if (functionSymbol.DefinitionSyntax != null)
                    Diagnostics.ReportSymbolRedefined(declaration.Name.SourceRange, functionSymbol);
                else
                    functionSymbol.DefinitionSyntax = declaration;
            }
            else
            {
                if (isQualifiedName)
                    Diagnostics.ReportUndeclaredFunctionInNamespaceOrClass((QualifiedDeclarationNameSyntax) declaration.Name);
                functionSymbol = new SourceFunctionSymbol(declaration, parent, boundReturnType.TypeSymbol);
                containerBinder.AddSymbol(functionSymbol, declaration.Name.SourceRange, true);
            }

            if (declaration.Semantic != null)
                Bind(declaration.Semantic, BindVariableQualifier);

            var functionBinder = (functionOwner != null && (functionOwner.Kind == SymbolKind.Class || functionOwner.Kind == SymbolKind.Struct))
                ? new StructMethodBinder(_sharedBinderState, this, (StructSymbol) functionOwner, functionSymbol)
                : new FunctionBinder(_sharedBinderState, this, functionSymbol);

            if (isQualifiedName)
                functionBinder = new ContainedFunctionBinder(_sharedBinderState, functionBinder, containerSymbol.Binder, functionSymbol);

            var boundParameters = BindParameters(declaration.ParameterList, functionBinder, functionSymbol);
            var boundBody = functionBinder.Bind(declaration.Body, x => functionBinder.BindBlock(x, functionSymbol));

            if (boundReturnType.Type != IntrinsicTypes.Void && !ReturnStatementWalker.ContainsReturnStatement(boundBody))
                Diagnostics.Report(declaration.Name.SourceRange, DiagnosticId.ReturnExpected, functionSymbol.Name);

            return new BoundFunctionDefinition(functionSymbol, boundReturnType, boundParameters.ToImmutableArray(), boundBody);
        }

        private sealed class ReturnStatementWalker : BoundTreeWalker
        {
            private bool _containsReturnStatement;

            /// <summary>
            /// TODO: This is just a hack. We need to proper flow analysis
            /// to detect whether all code paths return a value.
            /// </summary>
            public static bool ContainsReturnStatement(BoundBlock boundBlock)
            {
                var walker = new ReturnStatementWalker();
                walker.VisitBlock(boundBlock);
                return walker._containsReturnStatement;
            }

            protected override void VisitReturnStatement(BoundReturnStatement node)
            {
                base.VisitReturnStatement(node);

                _containsReturnStatement = true;
            }
        }

        private ImmutableArray<BoundVariableDeclaration> BindParameters(ParameterListSyntax parameterList, Binder invocableBinder, InvocableSymbol invocableSymbol)
        {
            var boundParameters = new List<BoundVariableDeclaration>();
            foreach (var parameterSyntax in parameterList.Parameters)
            {
                var parameterValueType = Bind(parameterSyntax.Type, x => BindType(x, null));
                var parameterDirection = SyntaxFacts.GetParameterDirection(parameterSyntax.Modifiers);

                boundParameters.Add(invocableBinder.Bind(parameterSyntax.Declarator, x => invocableBinder.BindVariableDeclarator(x, parameterValueType.TypeSymbol, (d, t) =>
                    new SourceParameterSymbol(
                        parameterSyntax,
                        invocableSymbol,
                        t,
                        parameterDirection))));
            }

            invocableSymbol.ClearParameters();
            foreach (var parameter in invocableBinder.LocalSymbols.Values.SelectMany(x => x))
                invocableSymbol.AddParameter((ParameterSymbol) parameter);

            return boundParameters.ToImmutableArray();
        }

        private BoundConstantBuffer BindConstantBufferDeclaration(ConstantBufferSyntax declaration)
        {
            var constantBufferSymbol = new ConstantBufferSymbol(declaration, null);

            var variables = new List<BoundMultipleVariableDeclarations>();

            // Add constant buffer fields to global scope.
            foreach (var field in declaration.Declarations)
            {
                var boundStatement = Bind(field, x => BindVariableDeclarationStatement(x, constantBufferSymbol));
                variables.Add(boundStatement);

                foreach (var boundDeclaration in boundStatement.VariableDeclarations)
                    constantBufferSymbol.AddMember(boundDeclaration.VariableSymbol);
            }

            return new BoundConstantBuffer(constantBufferSymbol, variables.ToImmutableArray());
        }

        private void BindBaseList(BaseListSyntax baseList, Symbol parent, out StructSymbol baseType, out List<InterfaceSymbol> baseInterfaces)
        {
            baseType = null;
            baseInterfaces = new List<InterfaceSymbol>();

            if (baseList != null)
            {
                var baseTypeTemp = Bind(baseList.BaseType, x => BindType(x, parent));
                switch (baseTypeTemp.TypeSymbol.Kind)
                {
                    case SymbolKind.Class:
                    case SymbolKind.Struct:
                        baseType = (StructSymbol) baseTypeTemp.TypeSymbol;
                        break;
                    case SymbolKind.Interface:
                        baseInterfaces.Add((InterfaceSymbol) baseTypeTemp.TypeSymbol);
                        break;
                }
            }
        }

        private BoundStructType BindStructDeclaration(StructTypeSyntax declaration, Symbol parent)
        {
            StructSymbol baseType;
            List<InterfaceSymbol> baseInterfaces;
            BindBaseList(declaration.BaseList, parent, out baseType, out baseInterfaces);

            var structBinder = new Binder(_sharedBinderState, this);
            var structSymbol = new StructSymbol(declaration, parent, baseType, baseInterfaces.ToImmutableArray(), structBinder);
            AddSymbol(structSymbol, declaration.Name?.SourceRange ?? declaration.SourceRange);

            var members = new List<BoundNode>();

            // Bit of a hack - need to add member symbols to structSymbol as we go, otherwise (for example)
            // static methods defined inline in a struct won't be able to see struct members.
            var alreadyAddedMemberSymbols = new List<Symbol>();
            Action addMemberSymbols = () =>
            {
                var newMemberSymbols = structBinder.LocalSymbols.Values
                    .SelectMany(x => x)
                    .Except(alreadyAddedMemberSymbols)
                    .ToList();
                foreach (var member in newMemberSymbols)
                {
                    structSymbol.AddMember(member);
                    alreadyAddedMemberSymbols.Add(member);
                }
            };

            foreach (var memberSyntax in declaration.Members)
            {
                switch (memberSyntax.Kind)
                {
                    case SyntaxKind.VariableDeclarationStatement:
                        members.Add(structBinder.Bind((VariableDeclarationStatementSyntax) memberSyntax, x => structBinder.BindField(x, structSymbol)));
                        addMemberSymbols();
                        break;
                    case SyntaxKind.FunctionDeclaration:
                        members.Add(structBinder.Bind((FunctionDeclarationSyntax) memberSyntax, x => structBinder.BindFunctionDeclaration(x, structSymbol)));
                        addMemberSymbols();
                        break;
                    case SyntaxKind.FunctionDefinition:
                        members.Add(structBinder.Bind((FunctionDefinitionSyntax) memberSyntax, x => structBinder.BindFunctionDefinition(x, structSymbol)));
                        addMemberSymbols();
                        break;
                }
            }

            return new BoundStructType(structSymbol, members.ToImmutableArray());
        }

        private BoundMultipleVariableDeclarations BindField(VariableDeclarationStatementSyntax variableDeclarationStatementSyntax, TypeSymbol parentType)
        {
            var declaration = variableDeclarationStatementSyntax.Declaration;
            return BindVariableDeclaration(declaration, parentType, (d, t) => new SourceFieldSymbol(d, parentType, t));
        }

        private BoundInterfaceType BindInterfaceDeclaration(InterfaceTypeSyntax declaration, Symbol parent)
        {
            var interfaceSymbol = new InterfaceSymbol(declaration, parent);
            AddSymbol(interfaceSymbol, declaration.Name.SourceRange);

            var methods = new List<BoundFunction>();
            var interfaceBinder = new Binder(_sharedBinderState, this);
            foreach (var memberSyntax in declaration.Methods)
                methods.Add(interfaceBinder.Bind(memberSyntax, x => interfaceBinder.BindFunctionDeclaration(x, interfaceSymbol)));

            foreach (var member in interfaceBinder.LocalSymbols.Values.SelectMany(x => x))
                interfaceSymbol.AddMember(member);

            return new BoundInterfaceType(interfaceSymbol, methods.ToImmutableArray());
        }

        private BoundTypeDeclaration BindTypeDeclaration(TypeDeclarationStatementSyntax declaration, Symbol parent)
        {
            return new BoundTypeDeclaration(Bind(declaration.Type, x => BindTypeDefinition(x, parent)));
        }

        private BoundType BindTypeDefinition(TypeDefinitionSyntax syntax, Symbol parent)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.ClassType:
                case SyntaxKind.StructType:
                    return BindStructDeclaration((StructTypeSyntax) syntax, parent);
                case SyntaxKind.InterfaceType:
                    return BindInterfaceDeclaration((InterfaceTypeSyntax) syntax, parent);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
