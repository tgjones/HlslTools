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
            var boundDeclarations = compilationUnit.Declarations.Select(x => Bind(x, BindGlobalDeclaration));
            return new BoundCompilationUnit(boundDeclarations.ToImmutableArray());
        }

        private BoundNode BindGlobalDeclaration(SyntaxNode declaration)
        {
            switch (declaration.Kind)
            {
                case SyntaxKind.VariableDeclarationStatement:
                    return BindVariableDeclaration((VariableDeclarationStatementSyntax) declaration);
                case SyntaxKind.FunctionDeclaration:
                    return BindFunctionDeclaration((FunctionDeclarationSyntax) declaration);
                case SyntaxKind.FunctionDefinition:
                    return BindFunctionDefinition((FunctionDefinitionSyntax) declaration);
                case SyntaxKind.ConstantBufferDeclaration:
                    return BindConstantBufferDeclaration((ConstantBufferSyntax) declaration);
                case SyntaxKind.TypeDeclarationStatement:
                    return BindTypeDeclaration((TypeDeclarationStatementSyntax) declaration);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private BoundMultipleVariableDeclarations BindVariableDeclaration(VariableDeclarationStatementSyntax variableDeclarationStatementSyntax)
        {
            var boundDeclarations = new List<BoundVariableDeclaration>();

            var declaration = variableDeclarationStatementSyntax.Declaration;
            foreach (var declarator in declaration.Variables)
            {
                var variableType = LookupSymbol(declaration.Type);

                foreach (var arrayRankSpecifier in declarator.ArrayRankSpecifiers)
                    variableType = new ArraySymbol(variableType);

                var symbol = new VariableSymbol(declarator, null, variableType);
                AddSymbol(symbol);

                var initializer = BindExpression(null); // TODO

                boundDeclarations.Add(new BoundVariableDeclaration(symbol, variableType, initializer));
            }

            return new BoundMultipleVariableDeclarations(boundDeclarations.ToImmutableArray());
        }

        private BoundFunctionDeclaration BindFunctionDeclaration(FunctionDeclarationSyntax declaration)
        {
            var returnType = LookupSymbol(declaration.ReturnType); // TODO

            Func<FunctionSymbol, IEnumerable<ParameterSymbol>> lazyParameterSymbols = fd =>
            {
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameterSyntax in declaration.ParameterList.Parameters)
                {
                    var parameterValueType = LookupSymbol(parameterSyntax.Type);
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
            AddSymbol(symbol);

            return new BoundFunctionDeclaration(symbol);
        }

        private BoundFunctionDefinition BindFunctionDefinition(FunctionDefinitionSyntax declaration)
        {
            var returnType = LookupSymbol(declaration.ReturnType); // TODO

            Func<FunctionSymbol, IEnumerable<ParameterSymbol>> lazyParameterSymbols = fd =>
            {
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameterSyntax in declaration.ParameterList.Parameters)
                {
                    var parameterValueType = LookupSymbol(parameterSyntax.Type);
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
            AddSymbol(symbol);

            return new BoundFunctionDefinition(symbol, BindBlock(declaration.Body));
        }

        private BoundConstantBuffer BindConstantBufferDeclaration(ConstantBufferSyntax declaration)
        {
            var variables = new List<BoundNode>();

            // Add constant buffer fields to global scope.
            foreach (var field in declaration.Declarations)
                variables.Add(BindVariableDeclaration((VariableDeclarationStatementSyntax) field));

            return new BoundConstantBuffer(variables.ToImmutableArray());
        }

        private BoundClassType BindClassDeclaration(ClassTypeSyntax declaration)
        {
            Func<TypeSymbol, IEnumerable<Symbol>> lazyMemberSymbols = cd =>
            {
                var memberSymbols = new List<Symbol>();
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
            AddSymbol(symbol);

            return new BoundClassType(symbol, ImmutableArray<BoundNode>.Empty); // TODO
        }

        private BoundStructType BindStructDeclaration(StructTypeSyntax declaration)
        {
            foreach (var field in declaration.Fields)
            {
                
            }

            Func<TypeSymbol, IEnumerable<FieldSymbol>> lazyMemberSymbols = cd =>
            {
                var memberSymbols = new List<FieldSymbol>();
                foreach (var memberSyntax in declaration.Fields)
                    memberSymbols.AddRange(BindFields(memberSyntax, cd));
                return memberSymbols;
            };

            var symbol = new StructSymbol(declaration, null, lazyMemberSymbols);
            AddSymbol(symbol);

            return new BoundStructType(symbol, ImmutableArray<BoundStatement>.Empty); // TODO
        }

        private BoundInterfaceType BindInterfaceDeclaration(InterfaceTypeSyntax declaration)
        {
            Func<TypeSymbol, IEnumerable<MethodDeclarationSymbol>> lazyMemberSymbols = cd =>
            {
                var memberSymbols = new List<MethodDeclarationSymbol>();
                foreach (var memberSyntax in declaration.Methods)
                    memberSymbols.Add(BindMethodDeclaration(memberSyntax, cd));
                return memberSymbols;
            };

            var symbol = new InterfaceSymbol(declaration, null, lazyMemberSymbols);
            AddSymbol(symbol);

            return new BoundInterfaceType(symbol, ImmutableArray<BoundNode>.Empty); // TODO
        }

        private IEnumerable<FieldSymbol> BindFields(VariableDeclarationStatementSyntax variableDeclarationStatementSyntax, TypeSymbol parentType)
        {
            var declaration = variableDeclarationStatementSyntax.Declaration;
            foreach (var declarator in declaration.Variables)
            {
                var variableType = LookupSymbol(declaration.Type);

                foreach (var arrayRankSpecifier in declarator.ArrayRankSpecifiers)
                    variableType = new ArraySymbol(variableType);

                var symbol = new SourceFieldSymbol(declarator, parentType, variableType);
                AddSymbol(symbol);

                yield return symbol;
            }
        }

        private MethodDeclarationSymbol BindMethodDeclaration(FunctionDeclarationSyntax declaration, TypeSymbol parentType)
        {
            var returnType = LookupSymbol(declaration.ReturnType); // TODO

            Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameterSymbols = fd =>
            {
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameterSyntax in declaration.ParameterList.Parameters)
                {
                    var parameterValueType = LookupSymbol(parameterSyntax.Type);
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
            AddSymbol(symbol);

            return symbol;
        }

        private MethodDefinitionSymbol BindMethodDefinition(FunctionDefinitionSyntax declaration, TypeSymbol parentType)
        {
            var returnType = LookupSymbol(declaration.ReturnType); // TODO

            Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameterSymbols = fd =>
            {
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameterSyntax in declaration.ParameterList.Parameters)
                {
                    var parameterValueType = LookupSymbol(parameterSyntax.Type);
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
            AddSymbol(symbol);

            return symbol;
        }

        private BoundNode BindTypeDeclaration(TypeDeclarationStatementSyntax declaration)
        {
            switch (declaration.Type.Kind)
            {
                case SyntaxKind.ClassType:
                    return BindClassDeclaration((ClassTypeSyntax) declaration.Type);
                case SyntaxKind.StructType:
                    return BindStructDeclaration((StructTypeSyntax) declaration.Type);
                case SyntaxKind.InterfaceType:
                    return BindInterfaceDeclaration((InterfaceTypeSyntax) declaration.Type);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}