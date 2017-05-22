using System;
using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Binding;
using ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Compilation
{
    public sealed class SemanticModel : SemanticModelBase
    {
        private readonly BindingResult _bindingResult;

        public Compilation Compilation { get; }

        public override SyntaxTreeBase SyntaxTree => Compilation.SyntaxTree;

        internal SemanticModel(Compilation compilation, BindingResult bindingResult)
        {
            Compilation = compilation;
            _bindingResult = bindingResult;
        }

        public override ISymbol GetDeclaredSymbol(SyntaxNodeBase declaration)
        {
            var node = (SyntaxNode) declaration;

            var parameter = node as ParameterSyntax;
            if (parameter != null)
                return GetDeclaredSymbol(parameter);

            var @namespace = node as NamespaceSyntax;
            if (@namespace != null)
                return GetDeclaredSymbol(@namespace);

            var interfaceType = node as InterfaceTypeSyntax;
            if (interfaceType != null)
                return GetDeclaredSymbol(interfaceType);

            var structType = node as StructTypeSyntax;
            if (structType != null)
                return GetDeclaredSymbol(structType);

            var variableDeclarator = node as VariableDeclaratorSyntax;
            if (variableDeclarator != null)
                return GetDeclaredSymbol(variableDeclarator);

            var typeAlias = node as TypeAliasSyntax;
            if (typeAlias != null)
                return GetDeclaredSymbol(typeAlias);

            var constantBuffer = node as ConstantBufferSyntax;
            if (constantBuffer != null)
                return GetDeclaredSymbol(constantBuffer);

            var functionDeclaration = node as FunctionDeclarationSyntax;
            if (functionDeclaration != null)
                return GetDeclaredSymbol(functionDeclaration);

            var functionDefinition = node as FunctionDefinitionSyntax;
            if (functionDefinition != null)
                return GetDeclaredSymbol(functionDefinition);

            var technique = node as TechniqueSyntax;
            if (technique != null)
                return GetDeclaredSymbol(technique);

            return null;
        }

        public ParameterSymbol GetDeclaredSymbol(ParameterSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax.Declarator) as BoundVariableDeclaration;
            return result?.VariableSymbol as ParameterSymbol;
        }

        public NamespaceSymbol GetDeclaredSymbol(NamespaceSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundNamespace;
            return result?.NamespaceSymbol;
        }

        public InterfaceSymbol GetDeclaredSymbol(InterfaceTypeSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundInterfaceType;
            return result?.InterfaceSymbol;
        }

        public StructSymbol GetDeclaredSymbol(StructTypeSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundStructType;
            return result?.StructSymbol;
        }

        public VariableSymbol GetDeclaredSymbol(VariableDeclaratorSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundVariableDeclaration;
            return result?.VariableSymbol;
        }

        public TypeAliasSymbol GetDeclaredSymbol(TypeAliasSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundTypeAlias;
            return result?.TypeAliasSymbol;
        }

        public ConstantBufferSymbol GetDeclaredSymbol(ConstantBufferSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundConstantBuffer;
            return result?.ConstantBufferSymbol;
        }

        public FunctionSymbol GetDeclaredSymbol(FunctionDeclarationSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundFunction;
            return result?.FunctionSymbol;
        }

        public FunctionSymbol GetDeclaredSymbol(FunctionDefinitionSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundFunction;
            return result?.FunctionSymbol;
        }

        public TechniqueSymbol GetDeclaredSymbol(TechniqueSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundTechnique;
            return result?.TechniqueSymbol;
        }

        public override SymbolInfo GetSymbolInfo(SyntaxNodeBase node)
        {
            Symbol getSymbol()
            {
                var identifierDeclarationName = node as IdentifierDeclarationNameSyntax;
                if (identifierDeclarationName != null)
                    return GetSymbol(identifierDeclarationName);

                var semantic = node as SemanticSyntax;
                if (semantic != null)
                    return GetSymbol(semantic);

                var attribute = node as AttributeSyntax;
                if (attribute != null)
                    return GetSymbol(attribute);

                var expression = node as ExpressionSyntax;
                if (expression != null)
                    return GetSymbol(expression);

                return null;
            }

            var symbol = getSymbol();
            return symbol != null
                ? new SymbolInfo(symbol)
                : SymbolInfo.None;
        }

        public Symbol GetSymbol(IdentifierDeclarationNameSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundName;
            return result?.Symbol;
        }

        public Symbol GetSymbol(SemanticSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundSemantic;
            return result?.SemanticSymbol;
        }

        public Symbol GetSymbol(AttributeSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundAttribute;
            return result?.AttributeSymbol;
        }

        public Symbol GetSymbol(ExpressionSyntax expression)
        {
            if (expression is IdentifierNameSyntax && expression.Parent.Kind == SyntaxKind.FunctionInvocationExpression)
                expression = (ExpressionSyntax) expression.Parent;

            var boundExpression = GetBoundExpression(expression);
            return boundExpression == null ? null : GetSymbol(boundExpression);
        }

        private static Symbol GetSymbol(BoundExpression expression)
        {
            switch (expression.Kind)
            {
                case BoundNodeKind.VariableExpression:
                    return GetSymbol((BoundVariableExpression) expression);
                case BoundNodeKind.NumericConstructorInvocationExpression:
                    return GetSymbol((BoundNumericConstructorInvocationExpression) expression);
                case BoundNodeKind.FunctionInvocationExpression:
                    return GetSymbol((BoundFunctionInvocationExpression) expression);
                case BoundNodeKind.MethodInvocationExpression:
                    return GetSymbol((BoundMethodInvocationExpression) expression);
                case BoundNodeKind.FieldExpression:
                    return GetSymbol((BoundFieldExpression) expression);
                case BoundNodeKind.Name:
                    return GetSymbol((BoundName) expression);
                case BoundNodeKind.IntrinsicGenericMatrixType:
                case BoundNodeKind.IntrinsicGenericVectorType:
                case BoundNodeKind.IntrinsicMatrixType:
                case BoundNodeKind.IntrinsicObjectType:
                case BoundNodeKind.IntrinsicScalarType:
                case BoundNodeKind.IntrinsicVectorType:
                    return GetSymbol((BoundType) expression);
                default:
                    // TODO: More bound expression types.
                    return null;
            }
        }

        private static Symbol GetSymbol(BoundVariableExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundNumericConstructorInvocationExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundFunctionInvocationExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundMethodInvocationExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundFieldExpression expression)
        {
            return expression.Field;
        }

        private static Symbol GetSymbol(BoundType expression)
        {
            return expression.TypeSymbol;
        }

        private static Symbol GetSymbol(BoundName expression)
        {
            return expression.Symbol;
        }

        public override TypeInfo GetTypeInfo(SyntaxNodeBase node)
        {
            var expression = node as ExpressionSyntax;
            if (expression != null)
            {
                var expressionType = GetExpressionType(expression);
                return expressionType != null
                    ? new TypeInfo(expressionType, expressionType)
                    : TypeInfo.None;
            }

            return TypeInfo.None;
        }

        public TypeSymbol GetExpressionType(ExpressionSyntax expression)
        {
            var boundExpression = GetBoundExpression(expression);
            return boundExpression?.Type;
        }

        private BoundExpression GetBoundExpression(ExpressionSyntax expression)
        {
            return _bindingResult.GetBoundNode(expression) as BoundExpression;
        }

        //public override IAliasSymbol GetAliasSymbol(SyntaxNodeBase node)
        //{
        //    var nameSyntax = node as IdentifierNameSyntax;
        //    return nameSyntax == null ? null : GetAliasInfo(nameSyntax);
        //}

        public override IEnumerable<Diagnostic> GetDiagnostics()
        {
            return _bindingResult.Diagnostics;
        }

        public IEnumerable<Symbol> LookupSymbols(SourceLocation position)
        {
            var node = FindClosestNodeWithBinder(_bindingResult.Root, position);
            var binder = node == null ? null : _bindingResult.GetBinder(node);
            return binder == null
                ? Enumerable.Empty<Symbol>()
                : LookupSymbols(binder);
        }

        private static IEnumerable<Symbol> LookupSymbols(Binder binder)
        {
            // NOTE: We want to only show the *available* symbols. That means, we need to
            //       hide symbols from the parent binder that have same name as the ones
            //       from a nested binder.
            //
            //       We do this by simply recording which names we've already seen.
            //       Please note that we *do* want to see duplicate names within the
            //       *same* binder.

            var allNames = new HashSet<string>();

            while (binder != null)
            {
                var localNames = new HashSet<string>();
                var localSymbols = binder.LocalSymbols
                    .SelectMany(x => x.Value)
                    .Where(s => !string.IsNullOrEmpty(s.Name));

                foreach (var symbol in localSymbols)
                {
                    if (!allNames.Contains(symbol.Name))
                    {
                        localNames.Add(symbol.Name);
                        yield return symbol;
                    }
                }

                allNames.UnionWith(localNames);
                binder = binder.Parent;
            }
        }

        private SyntaxNode FindClosestNodeWithBinder(SyntaxNode root, SourceLocation position)
        {
            var token = root.FindTokenContext(position);
            return (from n in token.Parent.AncestorsAndSelf().Cast<SyntaxNode>()
                let bc = _bindingResult.GetBinder(n)
                where bc != null
                select n).FirstOrDefault();
        }
    }
}