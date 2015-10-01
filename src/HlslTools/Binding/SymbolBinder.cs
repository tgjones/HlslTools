using System;
using System.Collections.Generic;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    internal sealed class SymbolBinder
    {
        private readonly BindingResult _bindingResult;

        public static void Bind(CompilationUnitSyntax compilationUnit, BindingResult bindingResult)
        {
            var binder = new SymbolBinder(bindingResult, null);
            binder.BindCompilationUnit(compilationUnit);
        }

        private readonly SymbolSet _symbolSet;

        private SymbolBinder(BindingResult bindingResult, ISymbolTable symbolTable)
        {
            _bindingResult = bindingResult;
            _symbolSet = new SymbolSet();
        }

        private void BindCompilationUnit(CompilationUnitSyntax compilationUnit)
        {
            foreach (var declaration in compilationUnit.Declarations)
            {
                switch (declaration.Kind)
                {
                    case SyntaxKind.VariableDeclarationStatement:
                        BindGlobalVariable((VariableDeclarationStatementSyntax) declaration);
                        break;
                    case SyntaxKind.FunctionDeclaration:
                        BindFunctionDeclaration((FunctionDeclarationSyntax)declaration);
                        break;
                    case SyntaxKind.FunctionDefinition:
                        BindFunctionDefinition((FunctionDefinitionSyntax) declaration);
                        break;
                    case SyntaxKind.ConstantBufferDeclaration:
                        BindConstantBufferDeclaration((ConstantBufferSyntax) declaration);
                        break;
                    case SyntaxKind.ClassType:
                        BindClassDeclaration((ClassTypeSyntax) declaration);
                        break;
                    case SyntaxKind.StructType:
                        BindStructDeclaration((StructTypeSyntax) declaration);
                        break;
                    case SyntaxKind.InterfaceType:
                        BindInterfaceDeclaration((InterfaceTypeSyntax) declaration);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void BindGlobalVariable(VariableDeclarationStatementSyntax variableDeclarationStatementSyntax)
        {
            var declaration = variableDeclarationStatementSyntax.Declaration;
            foreach (var declarator in declaration.Variables)
            {
                var variableType = _symbolSet.ResolveType(declaration.Type, null, null);

                foreach (var arrayRankSpecifier in declarator.ArrayRankSpecifiers)
                    variableType = new ArraySymbol(variableType);

                var symbol = new GlobalVariableSymbol(declarator, variableType);
                _bindingResult.AddSymbol(declarator, symbol);

                _symbolSet.AddGlobal(symbol);
            }
        }

        private void BindFunctionDeclaration(FunctionDeclarationSyntax declaration)
        {
            var returnType = _symbolSet.ResolveType(declaration.ReturnType, null, null);

            Func<FunctionSymbol, IEnumerable<ParameterSymbol>> lazyParameterSymbols = fd =>
            {
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameterSyntax in declaration.ParameterList.Parameters)
                {
                    var parameterValueType = _symbolSet.ResolveType(parameterSyntax.Type, null, null);
                    var parameterDirection = SyntaxFacts.GetParameterDirection(parameterSyntax.Modifiers);

                    parameterSymbols.Add(new SourceParameterSymbol(
                        parameterSyntax,
                        fd,
                        parameterValueType,
                        parameterDirection));
                }
                return parameterSymbols;
            };

            var symbol = new SourceFunctionDeclarationSymbol(declaration, returnType, lazyParameterSymbols);
            _bindingResult.AddSymbol(declaration, symbol);

            _symbolSet.AddGlobal(symbol);
        }

        private void BindFunctionDefinition(FunctionDefinitionSyntax declaration)
        {
            var returnType = _symbolSet.ResolveType(declaration.ReturnType, null, null);

            Func<FunctionSymbol, IEnumerable<ParameterSymbol>> lazyParameterSymbols = fd =>
            {
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameterSyntax in declaration.ParameterList.Parameters)
                {
                    var parameterValueType = _symbolSet.ResolveType(parameterSyntax.Type, null, null);
                    var parameterDirection = SyntaxFacts.GetParameterDirection(parameterSyntax.Modifiers);

                    parameterSymbols.Add(new SourceParameterSymbol(
                        parameterSyntax,
                        fd,
                        parameterValueType,
                        parameterDirection));
                }
                return parameterSymbols;
            };

            var symbol = new SourceFunctionDefinitionSymbol(declaration, returnType, lazyParameterSymbols);
            _bindingResult.AddSymbol(declaration, symbol);

            _symbolSet.AddGlobal(symbol);
        }

        private void BindConstantBufferDeclaration(ConstantBufferSyntax declaration)
        {
            // Add constant buffer fields to global scope.
            throw new NotImplementedException();
            //foreach (var field in declaration.Declarations)
            //    BindGlobalVariable(field);
        }

        private void BindClassDeclaration(ClassTypeSyntax declaration)
        {
            Func<TypeSymbol, IEnumerable<MemberSymbol>> lazyMemberSymbols = cd =>
            {
                var memberSymbols = new List<MemberSymbol>();
                foreach (var memberSyntax in declaration.Members)
                {
                    switch (memberSyntax.Kind)
                    {
                        case SyntaxKind.VariableDeclarationStatement:
                            memberSymbols.AddRange(BindFields((VariableDeclarationStatementSyntax)memberSyntax, cd));
                            break;
                        case SyntaxKind.FunctionDeclaration:
                            memberSymbols.Add(BindMethodDeclaration((FunctionDeclarationSyntax)memberSyntax, cd));
                            break;
                        case SyntaxKind.FunctionDefinition:
                            memberSymbols.Add(BindMethodDefinition((FunctionDefinitionSyntax)memberSyntax, cd));
                            break;
                    }
                }
                return memberSymbols;
            };

            var symbol = new ClassSymbol(declaration, null, lazyMemberSymbols);
            _bindingResult.AddSymbol(declaration, symbol);

            _symbolSet.AddGlobal(symbol);
        }

        private void BindStructDeclaration(StructTypeSyntax declaration)
        {
            Func<TypeSymbol, IEnumerable<FieldSymbol>> lazyMemberSymbols = cd =>
            {
                var memberSymbols = new List<FieldSymbol>();
                foreach (var memberSyntax in declaration.Fields)
                    memberSymbols.AddRange(BindFields(memberSyntax, cd));
                return memberSymbols;
            };

            var symbol = new StructSymbol(declaration, null, lazyMemberSymbols);
            _bindingResult.AddSymbol(declaration, symbol);

            _symbolSet.AddGlobal(symbol);
        }

        private void BindInterfaceDeclaration(InterfaceTypeSyntax declaration)
        {
            Func<TypeSymbol, IEnumerable<MethodDeclarationSymbol>> lazyMemberSymbols = cd =>
            {
                var memberSymbols = new List<MethodDeclarationSymbol>();
                foreach (var memberSyntax in declaration.Methods)
                    memberSymbols.Add(BindMethodDeclaration(memberSyntax, cd));
                return memberSymbols;
            };

            var symbol = new InterfaceSymbol(declaration, null, lazyMemberSymbols);
            _bindingResult.AddSymbol(declaration, symbol);

            _symbolSet.AddGlobal(symbol);
        }

        private IEnumerable<FieldSymbol> BindFields(VariableDeclarationStatementSyntax variableDeclarationStatementSyntax, TypeSymbol parentType)
        {
            var declaration = variableDeclarationStatementSyntax.Declaration;
            foreach (var declarator in declaration.Variables)
            {
                var variableType = _symbolSet.ResolveType(declaration.Type, null, null);

                foreach (var arrayRankSpecifier in declarator.ArrayRankSpecifiers)
                    variableType = new ArraySymbol(variableType);

                var symbol = new SourceFieldSymbol(declarator, parentType, variableType);
                _bindingResult.AddSymbol(declarator, symbol);

                yield return symbol;
            }
        }

        private MethodDeclarationSymbol BindMethodDeclaration(FunctionDeclarationSyntax declaration, TypeSymbol parentType)
        {
            var returnType = _symbolSet.ResolveType(declaration.ReturnType, parentType, null);

            Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameterSymbols = fd =>
            {
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameterSyntax in declaration.ParameterList.Parameters)
                {
                    var parameterValueType = _symbolSet.ResolveType(parameterSyntax.Type, null, null);
                    var parameterDirection = SyntaxFacts.GetParameterDirection(parameterSyntax.Modifiers);

                    parameterSymbols.Add(new SourceParameterSymbol(
                        parameterSyntax,
                        fd,
                        parameterValueType,
                        parameterDirection));
                }
                return parameterSymbols;
            };

            var symbol = new SourceMethodDeclarationSymbol(declaration, parentType, returnType, lazyParameterSymbols);
            _bindingResult.AddSymbol(declaration, symbol);

            return symbol;
        }

        private MethodDefinitionSymbol BindMethodDefinition(FunctionDefinitionSyntax declaration, TypeSymbol parentType)
        {
            var returnType = _symbolSet.ResolveType(declaration.ReturnType, parentType, null);

            Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameterSymbols = fd =>
            {
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameterSyntax in declaration.ParameterList.Parameters)
                {
                    var parameterValueType = _symbolSet.ResolveType(parameterSyntax.Type, null, null);
                    var parameterDirection = SyntaxFacts.GetParameterDirection(parameterSyntax.Modifiers);

                    parameterSymbols.Add(new SourceParameterSymbol(
                        parameterSyntax,
                        fd,
                        parameterValueType,
                        parameterDirection));
                }
                return parameterSymbols;
            };

            var symbol = new SourceMethodDefinitionSymbol(declaration, parentType, returnType, lazyParameterSymbols);
            _bindingResult.AddSymbol(declaration, symbol);

            return symbol;
        }
    }
}