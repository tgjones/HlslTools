using System.Collections.Generic;
using ShaderTools.Hlsl.Binding.BoundNodes;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding
{
    internal sealed class FunctionImplementationChecker : BoundTreeWalker
    {
        private readonly List<Diagnostic> _diagnostics;

        public FunctionImplementationChecker(List<Diagnostic> diagnostics)
        {
            _diagnostics = diagnostics;
        }

        protected override void VisitFunctionInvocationExpression(BoundFunctionInvocationExpression node)
        {
            var sourceFunctionSymbol = node.Symbol as SourceFunctionSymbol;
            if (sourceFunctionSymbol != null)
            {
                if (sourceFunctionSymbol.DefinitionSyntax == null && !IsInterfaceMethod(sourceFunctionSymbol))
                    _diagnostics.ReportFunctionMissingImplementation(node.Syntax);
            }

            base.VisitFunctionInvocationExpression(node);
        }

        protected override void VisitMethodInvocationExpression(BoundMethodInvocationExpression node)
        {
            var sourceMethodSymbol = node.Symbol as SourceFunctionSymbol;
            if (sourceMethodSymbol != null)
            {
                if (sourceMethodSymbol.DefinitionSyntax == null && !IsInterfaceMethod(sourceMethodSymbol))
                    _diagnostics.ReportMethodMissingImplementation(node.Syntax);
            }

            base.VisitMethodInvocationExpression(node);
        }

        private static bool IsInterfaceMethod(SourceFunctionSymbol symbol)
        {
            return symbol.Parent != null && symbol.Parent.Kind == SymbolKind.Interface;
        }
    }
}