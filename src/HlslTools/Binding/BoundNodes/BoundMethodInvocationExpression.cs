using System.Collections.Generic;
using System.Collections.Immutable;
using HlslTools.Binding.Signatures;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundMethodInvocationExpression : BoundExpression
    {
        public BoundMethodInvocationExpression(MethodInvocationExpressionSyntax syntax, BoundExpression target, ImmutableArray<BoundExpression> arguments, OverloadResolutionResult<FunctionSymbolSignature> result)
            : base(BoundNodeKind.MethodInvocationExpression)
        {
            Syntax = syntax;
            Target = target;
            Arguments = arguments;
            Result = result;
        }

        public override TypeSymbol Type => Symbol == null ? TypeFacts.Unknown : Symbol.ReturnType;

        public FunctionSymbol Symbol => Result.Selected?.Signature.Symbol;

        public MethodInvocationExpressionSyntax Syntax { get; }
        public BoundExpression Target { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }

        public OverloadResolutionResult<FunctionSymbolSignature> Result { get; }

        public BoundMethodInvocationExpression Update(IEnumerable<BoundExpression> arguments, OverloadResolutionResult<FunctionSymbolSignature> result)
        {
            var newArguments = arguments.ToImmutableArray();

            if (newArguments == Arguments && result == Result)
                return this;

            return new BoundMethodInvocationExpression(Syntax, Target, newArguments, result);
        }

        public override string ToString()
        {
            return $"{Symbol.Name}({string.Join(",", Arguments)})";
        }
    }
}