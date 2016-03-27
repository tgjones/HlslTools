using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding.BoundNodes;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding
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
                    return BindVariableDeclarationStatement((VariableDeclarationStatementSyntax) declaration);
                case SyntaxKind.FunctionDeclaration:
                    return BindFunctionDeclaration((FunctionDeclarationSyntax) declaration, parent);
                case SyntaxKind.FunctionDefinition:
                    return BindFunctionDefinition((FunctionDefinitionSyntax) declaration, parent);
                case SyntaxKind.ConstantBufferDeclaration:
                    return BindConstantBufferDeclaration((ConstantBufferSyntax) declaration);
                case SyntaxKind.TypeDeclarationStatement:
                    return BindTypeDeclaration((TypeDeclarationStatementSyntax) declaration);
                case SyntaxKind.Namespace:
                    return BindNamespace((NamespaceSyntax) declaration);
                case SyntaxKind.TechniqueDeclaration:
                    return BindTechniqueDeclaration((TechniqueSyntax) declaration);
                default:
                    throw new ArgumentOutOfRangeException(declaration.Kind.ToString());
            }
        }

        private BoundNode BindTechniqueDeclaration(TechniqueSyntax declaration)
        {
            var techniqueBinder = new Binder(_sharedBinderState, this);
            var boundPasses = declaration.Passes.Select(x => techniqueBinder.Bind(x, techniqueBinder.BindPass));
            return new BoundTechnique(boundPasses.ToImmutableArray());
        }

        private BoundPass BindPass(PassSyntax syntax)
        {
            return new BoundPass();
        }

        private BoundNode BindNamespace(NamespaceSyntax declaration)
        {
            var enclosingNamespace = LookupEnclosingNamespace();
            var namespaceSymbol = new NamespaceSymbol(declaration, enclosingNamespace);

            AddSymbol(namespaceSymbol, declaration.Name.Span);

            var namespaceBinder = new NamespaceBinder(_sharedBinderState, this, namespaceSymbol);
            namespaceSymbol.Binder = namespaceBinder;

            var boundDeclarations = namespaceBinder.BindTopLevelDeclarations(declaration.Declarations, namespaceSymbol);

            foreach (var member in namespaceBinder.LocalSymbols)
                namespaceSymbol.AddMember(member);

            return new BoundNamespace(namespaceSymbol, boundDeclarations);
        }

        private BoundMultipleVariableDeclarations BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            return BindVariableDeclaration(syntax, (d, t) => new VariableSymbol(d, null, t));
        }

        private BoundMultipleVariableDeclarations BindVariableDeclaration(VariableDeclarationSyntax syntax, Func<VariableDeclaratorSyntax, TypeSymbol, VariableSymbol> createSymbol)
        {
            var boundDeclarations = new List<BoundVariableDeclaration>();

            switch (syntax.Type.Kind)
            {
                case SyntaxKind.StructType:
                    var structType = (StructTypeSyntax) syntax.Type;
                    Bind(structType, BindStructDeclaration);
                    break;
            }

            foreach (var declarator in syntax.Variables)
            {
                var variableType = LookupType(syntax.Type);
                boundDeclarations.Add(Bind(declarator, x => BindVariableDeclarator(x, variableType, createSymbol)));
            }

            return new BoundMultipleVariableDeclarations(boundDeclarations.ToImmutableArray());
        }

        private BoundVariableDeclaration BindVariableDeclarator(VariableDeclaratorSyntax syntax, TypeSymbol variableType, Func<VariableDeclaratorSyntax, TypeSymbol, VariableSymbol> createSymbol)
        {
            variableType = BindArrayRankSpecifiers(syntax, variableType);

            var symbol = createSymbol(syntax, variableType);
            AddSymbol(symbol, syntax.Identifier.Span);

            BoundInitializer initializer = null;
            if (syntax.Initializer != null)
                initializer = BindInitializer(syntax.Initializer);

            return new BoundVariableDeclaration(symbol, variableType, initializer);
        }

        private TypeSymbol BindArrayRankSpecifiers(VariableDeclaratorSyntax declarator, TypeSymbol variableType)
        {
            foreach (var arrayRankSpecifier in declarator.ArrayRankSpecifiers)
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
            var returnType = LookupType(declaration.ReturnType);

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
                functionSymbol = new SourceFunctionSymbol(declaration, parent, returnType);
                AddSymbol(functionSymbol, declaration.Name.GetTextSpanSafe(), true);
            }

            var functionBinder = new Binder(_sharedBinderState, this);

            var boundParameters = BindParameters(declaration.ParameterList, functionBinder, functionSymbol);

            return new BoundFunctionDeclaration(functionSymbol, boundParameters.ToImmutableArray());
        }

        private BoundNode BindFunctionDefinition(FunctionDefinitionSyntax declaration, Symbol parent)
        {
            var returnType = LookupType(declaration.ReturnType);

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
                    containerSymbol = LookupContainer((QualifiedDeclarationNameSyntax) declaration.Name);
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

            var functionSymbol = symbolTable.OfType<SourceFunctionSymbol>()
                .FirstOrDefault(x => SyntaxFacts.HaveMatchingSignatures(
                    x.DefinitionSyntax as FunctionSyntax ?? x.DeclarationSyntaxes[0],
                    declaration));

            if (functionSymbol != null)
            {
                if (functionSymbol.DefinitionSyntax != null)
                    Diagnostics.ReportSymbolRedefined(declaration.Name.GetTextSpanSafe(), functionSymbol);
                else
                    functionSymbol.DefinitionSyntax = declaration;
            }
            else
            {
                if (isQualifiedName)
                    Diagnostics.ReportUndeclaredFunctionInNamespaceOrClass((QualifiedDeclarationNameSyntax) declaration.Name);
                functionSymbol = new SourceFunctionSymbol(declaration, parent, returnType);
                containerBinder.AddSymbol(functionSymbol, declaration.Name.GetTextSpanSafe(), true);
            }

            var functionBinder = (functionOwner != null && functionOwner.Kind == SymbolKind.Class)
                ? new ClassMethodBinder(_sharedBinderState, this, (ClassSymbol) functionOwner)
                : new Binder(_sharedBinderState, this);

            if (isQualifiedName)
                functionBinder = new ContainedFunctionBinder(_sharedBinderState, functionBinder, containerSymbol.Binder);

            var boundParameters = BindParameters(declaration.ParameterList, functionBinder, functionSymbol);
            var boundBody = functionBinder.Bind(declaration.Body, functionBinder.BindBlock);

            return new BoundFunctionDefinition(functionSymbol, boundParameters.ToImmutableArray(), boundBody);
        }

        private ImmutableArray<BoundVariableDeclaration> BindParameters(ParameterListSyntax parameterList, Binder invocableBinder, InvocableSymbol invocableSymbol)
        {
            var boundParameters = new List<BoundVariableDeclaration>();
            foreach (var parameterSyntax in parameterList.Parameters)
            {
                var parameterValueType = LookupType(parameterSyntax.Type);
                var parameterDirection = SyntaxFacts.GetParameterDirection(parameterSyntax.Modifiers);

                boundParameters.Add(invocableBinder.BindVariableDeclarator(parameterSyntax.Declarator, parameterValueType, (d, t) =>
                    new SourceParameterSymbol(
                        parameterSyntax,
                        invocableSymbol,
                        t,
                        parameterDirection)));
            }

            invocableSymbol.ClearParameters();
            foreach (var parameter in invocableBinder.LocalSymbols)
                invocableSymbol.AddParameter((ParameterSymbol) parameter);

            return boundParameters.ToImmutableArray();
        }

        private BoundConstantBuffer BindConstantBufferDeclaration(ConstantBufferSyntax declaration)
        {
            var variables = new List<BoundMultipleVariableDeclarations>();

            // Add constant buffer fields to global scope.
            foreach (var field in declaration.Declarations)
                variables.Add(BindVariableDeclarationStatement(field));

            return new BoundConstantBuffer(variables.ToImmutableArray());
        }

        private BoundClassType BindClassDeclaration(ClassTypeSyntax declaration)
        {
            ClassSymbol baseClass = null;
            var baseInterfaces = new List<InterfaceSymbol>();

            // TODO: HLSL allows zero or one base class, and zero or more implemented interfaces.

            if (declaration.BaseList != null)
            {
                var baseType = LookupType(declaration.BaseList.BaseType);
                switch (baseType.Kind)
                {
                    case SymbolKind.Class:
                        baseClass = (ClassSymbol) baseType;
                        break;
                    case SymbolKind.Interface:
                        baseInterfaces.Add((InterfaceSymbol) baseType);
                        break;
                }
            }

            var classBinder = new Binder(_sharedBinderState, this);

            var classSymbol = new ClassSymbol(declaration, null, baseClass, baseInterfaces.ToImmutableArray(), classBinder); // TODO: parent symbol could be namespace or function body
            AddSymbol(classSymbol, declaration.Name.Span);

            var members = new List<BoundNode>();

            foreach (var memberSyntax in declaration.Members)
            {
                switch (memberSyntax.Kind)
                {
                    case SyntaxKind.VariableDeclarationStatement:
                        members.Add(classBinder.Bind((VariableDeclarationStatementSyntax) memberSyntax, classBinder.BindVariableDeclarationStatement));
                        break;
                    case SyntaxKind.FunctionDeclaration:
                        members.Add(classBinder.Bind((FunctionDeclarationSyntax) memberSyntax, x => classBinder.BindFunctionDeclaration(x, classSymbol)));
                        break;
                    case SyntaxKind.FunctionDefinition:
                        members.Add(classBinder.Bind((FunctionDefinitionSyntax) memberSyntax, x => classBinder.BindFunctionDefinition(x, classSymbol)));
                        break;
                }
            }

            foreach (var member in classBinder.LocalSymbols)
                classSymbol.AddMember(member);

            return new BoundClassType(classSymbol, members.ToImmutableArray());
        }

        private BoundStructType BindStructDeclaration(StructTypeSyntax declaration)
        {
            var structSymbol = new StructSymbol(declaration, null);
            AddSymbol(structSymbol, declaration.Name?.Span ?? declaration.GetTextSpanSafe());

            var variables = new List<BoundMultipleVariableDeclarations>();
            var structBinder = new Binder(_sharedBinderState, this);
            foreach (var variableDeclarationStatement in declaration.Fields)
                variables.Add(structBinder.Bind(variableDeclarationStatement, x => structBinder.BindField(x, structSymbol)));

            foreach (var member in structBinder.LocalSymbols)
                structSymbol.AddMember(member);

            return new BoundStructType(structSymbol, variables.ToImmutableArray());
        }

        private BoundMultipleVariableDeclarations BindField(VariableDeclarationStatementSyntax variableDeclarationStatementSyntax, TypeSymbol parentType)
        {
            var declaration = variableDeclarationStatementSyntax.Declaration;
            return BindVariableDeclaration(declaration, (d, t) => new SourceFieldSymbol(d, parentType, t));
        }

        private BoundInterfaceType BindInterfaceDeclaration(InterfaceTypeSyntax declaration)
        {
            var interfaceSymbol = new InterfaceSymbol(declaration, null);
            AddSymbol(interfaceSymbol, declaration.Name.Span);

            var methods = new List<BoundFunction>();
            var interfaceBinder = new Binder(_sharedBinderState, this);
            foreach (var memberSyntax in declaration.Methods)
                methods.Add(interfaceBinder.Bind(memberSyntax, x => interfaceBinder.BindFunctionDeclaration(x, interfaceSymbol)));

            foreach (var member in interfaceBinder.LocalSymbols)
                interfaceSymbol.AddMember(member);

            return new BoundInterfaceType(interfaceSymbol, methods.ToImmutableArray());
        }

        private BoundTypeDeclaration BindTypeDeclaration(TypeDeclarationStatementSyntax declaration)
        {
            return new BoundTypeDeclaration(Bind(declaration.Type, BindTypeDefinition));
        }

        private BoundType BindTypeDefinition(TypeDefinitionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.ClassType:
                    return BindClassDeclaration((ClassTypeSyntax) syntax);
                case SyntaxKind.StructType:
                    return BindStructDeclaration((StructTypeSyntax) syntax);
                case SyntaxKind.InterfaceType:
                    return BindInterfaceDeclaration((InterfaceTypeSyntax) syntax);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}