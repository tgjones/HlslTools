using System.Collections.Generic;
using HlslTools.Binding.BoundNodes;
using HlslTools.Diagnostics;
using HlslTools.Symbols;

namespace HlslTools.Binding
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
                if (sourceFunctionSymbol.DefinitionSyntax == null)
                    _diagnostics.ReportFunctionMissingImplementation(node.Syntax);
            }

            base.VisitFunctionInvocationExpression(node);
        }

        protected override void VisitMethodInvocationExpression(BoundMethodInvocationExpression node)
        {
            var sourceMethodSymbol = node.Symbol as SourceFunctionSymbol;
            if (sourceMethodSymbol != null)
            {
                if (sourceMethodSymbol.DefinitionSyntax == null)
                    _diagnostics.ReportMethodMissingImplementation(node.Syntax);
            }

            base.VisitMethodInvocationExpression(node);
        }
    }
}