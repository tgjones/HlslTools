using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding.BoundNodes;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    internal partial class Binder
    {
        private BoundCompilationUnit BindCompilationUnit(CompilationUnitSyntax compilationUnit)
        {
            return new BoundCompilationUnit(BindTopLevelDeclarations(compilationUnit.Declarations));
        }

        private ImmutableArray<BoundNode> BindTopLevelDeclarations(List<SyntaxNode> declarations)
        {
            return declarations.Select(x => Bind(x, BindGlobalDeclaration)).ToImmutableArray();
        }

        private BoundNode BindGlobalDeclaration(SyntaxNode declaration)
        {
            switch (declaration.Kind)
            {
                case SyntaxKind.VariableDeclarationStatement:
                    return BindVariableDeclarationStatement((VariableDeclarationStatementSyntax) declaration);
                case SyntaxKind.FunctionDeclaration:
                    return BindFunctionDeclaration((FunctionDeclarationSyntax) declaration);
                case SyntaxKind.FunctionDefinition:
                    return BindFunctionDefinition((FunctionDefinitionSyntax) declaration);
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
            // TODO
            return new BoundPass();
        }

        private BoundNode BindNamespace(NamespaceSyntax declaration)
        {
            var enclosingNamespace = LookupEnclosingNamespace();
            var namespaceSymbol = new NamespaceSymbol(declaration.Name.Text, enclosingNamespace);

            AddSymbol(namespaceSymbol);

            var namespaceBinder = new NamespaceBinder(_sharedBinderState, this, namespaceSymbol);
            namespaceBinder.BindTopLevelDeclarations(declaration.Declarations);

            return new BoundNamespace(namespaceSymbol);
        }

        private BoundMultipleVariableDeclarations BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var boundDeclarations = new List<BoundVariableDeclaration>();

            foreach (var declarator in syntax.Variables)
            {
                var variableType = LookupType(syntax.Type);

                foreach (var arrayRankSpecifier in declarator.ArrayRankSpecifiers)
                    variableType = new ArraySymbol(variableType);

                var symbol = new VariableSymbol(declarator, null, variableType);
                AddSymbol(symbol);

                BoundInitializer initializer = null;
                if (declarator.Initializer != null)
                    initializer = BindInitializer(declarator.Initializer);

                boundDeclarations.Add(Bind(declarator, x => new BoundVariableDeclaration(symbol, variableType, initializer)));
            }

            return new BoundMultipleVariableDeclarations(boundDeclarations.ToImmutableArray());
        }

        private BoundFunction BindFunctionDeclaration(FunctionDeclarationSyntax declaration)
        {
            var returnType = LookupType(declaration.ReturnType);

            Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameterSymbols = fd =>
            {
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameterSyntax in declaration.ParameterList.Parameters)
                {
                    var parameterValueType = LookupType(parameterSyntax.Type);
                    var parameterDirection = SyntaxFacts.GetParameterDirection(parameterSyntax.Modifiers);

                    parameterSymbols.Add(new SourceParameterSymbol(
                        parameterSyntax,
                        fd,
                        parameterValueType,
                        parameterDirection));
                }
                return parameterSymbols;
            };

            var symbol = new SourceFunctionSymbol(declaration, returnType, lazyParameterSymbols);
            AddSymbol(symbol);

            return new BoundFunction(symbol);
        }

        private BoundFunction BindFunctionDefinition(FunctionDefinitionSyntax declaration)
        {
            var returnType = LookupType(declaration.ReturnType);

            var functionBinder = new Binder(_sharedBinderState, this);
            
            var functionSymbol = new SourceFunctionSymbol(declaration, returnType);
            AddSymbol(functionSymbol);

            foreach (var parameterSyntax in declaration.ParameterList.Parameters)
            {
                var parameterValueType = LookupType(parameterSyntax.Type);
                var parameterDirection = SyntaxFacts.GetParameterDirection(parameterSyntax.Modifiers);

                var parameterSymbol = new SourceParameterSymbol(
                    parameterSyntax,
                    functionSymbol,
                    parameterValueType,
                    parameterDirection);

                functionSymbol.AddParameter(parameterSymbol);

                functionBinder.AddSymbol(parameterSymbol);
            }

            functionBinder.Bind(declaration.Body, functionBinder.BindBlock);

            return new BoundFunction(functionSymbol);
        }

        private BoundConstantBuffer BindConstantBufferDeclaration(ConstantBufferSyntax declaration)
        {
            var variables = new List<BoundNode>();

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

            var classSymbol = new ClassSymbol(declaration, null, baseClass, baseInterfaces.ToImmutableArray()); // TODO: parent symbol could be namespace or function body
            AddSymbol(classSymbol);

            foreach (var memberSyntax in declaration.Members)
            {
                switch (memberSyntax.Kind)
                {
                    case SyntaxKind.VariableDeclarationStatement:
                        foreach (var field in BindFields((VariableDeclarationStatementSyntax)memberSyntax, classSymbol))
                            classSymbol.AddMember(field);
                        break;
                    case SyntaxKind.FunctionDeclaration:
                        classSymbol.AddMember(BindMethodDeclaration((FunctionDeclarationSyntax)memberSyntax, classSymbol));
                        break;
                    case SyntaxKind.FunctionDefinition:
                        classSymbol.AddMember(BindMethodDefinition((FunctionDefinitionSyntax)memberSyntax, classSymbol));
                        break;
                }
            }

            return new BoundClassType(classSymbol);
        }

        private BoundStructType BindStructDeclaration(StructTypeSyntax declaration)
        {
            var structSymbol = new StructSymbol(declaration, null);
            AddSymbol(structSymbol);

            var structBinder = new Binder(_sharedBinderState, this);
            foreach (var memberSyntax in declaration.Fields)
                foreach (var fieldSymbol in structBinder.BindFields(memberSyntax, structSymbol))
                    structSymbol.AddMember(fieldSymbol);

            return new BoundStructType(structSymbol);
        }

        private IEnumerable<FieldSymbol> BindFields(VariableDeclarationStatementSyntax variableDeclarationStatementSyntax, TypeSymbol parentType)
        {
            var declaration = variableDeclarationStatementSyntax.Declaration;
            foreach (var declarator in declaration.Variables)
            {
                var variableType = LookupType(declaration.Type);

                foreach (var arrayRankSpecifier in declarator.ArrayRankSpecifiers)
                    variableType = new ArraySymbol(variableType);

                yield return new SourceFieldSymbol(declarator, parentType, variableType);
            }
        }

        private BoundInterfaceType BindInterfaceDeclaration(InterfaceTypeSyntax declaration)
        {
            var interfaceSymbol = new InterfaceSymbol(declaration, null);
            AddSymbol(interfaceSymbol);

            foreach (var memberSyntax in declaration.Methods)
                interfaceSymbol.AddMember(BindMethodDeclaration(memberSyntax, interfaceSymbol));

            return new BoundInterfaceType(interfaceSymbol);
        }

        private MethodSymbol BindMethodDeclaration(FunctionDeclarationSyntax declaration, TypeSymbol parentType)
        {
            var returnType = LookupType(declaration.ReturnType);

            Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameterSymbols = fd =>
            {
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameterSyntax in declaration.ParameterList.Parameters)
                {
                    var parameterValueType = LookupType(parameterSyntax.Type);
                    var parameterDirection = SyntaxFacts.GetParameterDirection(parameterSyntax.Modifiers);

                    parameterSymbols.Add(new SourceParameterSymbol(
                        parameterSyntax,
                        fd,
                        parameterValueType,
                        parameterDirection));
                }
                return parameterSymbols;
            };

            var symbol = new SourceMethodSymbol(declaration, parentType, returnType, lazyParameterSymbols);
            AddSymbol(symbol);

            return symbol;
        }

        private MethodSymbol BindMethodDefinition(FunctionDefinitionSyntax declaration, TypeSymbol parentType)
        {
            var returnType = LookupType(declaration.ReturnType);

            Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameterSymbols = fd =>
            {
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameterSyntax in declaration.ParameterList.Parameters)
                {
                    var parameterValueType = LookupType(parameterSyntax.Type);
                    var parameterDirection = SyntaxFacts.GetParameterDirection(parameterSyntax.Modifiers);

                    parameterSymbols.Add(new SourceParameterSymbol(
                        parameterSyntax,
                        fd,
                        parameterValueType,
                        parameterDirection));
                }
                return parameterSymbols;
            };

            var symbol = new SourceMethodSymbol(declaration, parentType, returnType, lazyParameterSymbols);
            AddSymbol(symbol);

            return symbol;
        }

        private BoundTypeDeclaration BindTypeDeclaration(TypeDeclarationStatementSyntax declaration)
        {
            return new BoundTypeDeclaration(Bind(declaration.Type, BindTypeDefinition));
        }

        private BoundNode BindTypeDefinition(TypeDefinitionSyntax syntax)
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