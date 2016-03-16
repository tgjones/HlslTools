using System.Collections.Generic;
using System.Collections.Immutable;
using HlslTools.Binding.Signatures;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundMethodInvocationExpression : BoundExpression
    {
        public BoundMethodInvocationExpression(BoundExpression target, ImmutableArray<BoundExpression> arguments, OverloadResolutionResult<MethodSymbolSignature> result)
            : base(BoundNodeKind.MethodInvocationExpression)
        {
            Target = target;
            Arguments = arguments;
            Result = result;
        }

        public override TypeSymbol Type => Symbol?.ReturnType;

        public MethodSymbol Symbol => Result.Selected?.Signature.Symbol;

        public BoundExpression Target { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }

        public OverloadResolutionResult<MethodSymbolSignature> Result { get; }

        public BoundMethodInvocationExpression Update(IEnumerable<BoundExpression> arguments, OverloadResolutionResult<MethodSymbolSignature> result)
        {
            var newArguments = arguments.ToImmutableArray();

            if (newArguments == Arguments && result == Result)
                return this;

            return new BoundMethodInvocationExpression(Target, newArguments, result);
        }

        public override string ToString()
        {
            return $"{Symbol.Name}({string.Join(",", Arguments)})";
        }
    }
}