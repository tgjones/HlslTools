using System.Collections.Generic;
using System.Collections.Immutable;
using HlslTools.Binding.Signatures;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundFunctionInvocationExpression : BoundExpression
    {
        public BoundFunctionInvocationExpression(SyntaxNode syntax, IEnumerable<BoundExpression> arguments, OverloadResolutionResult<FunctionSymbolSignature> result)
            : base(BoundNodeKind.FunctionInvocationExpression, syntax)
        {
            Arguments = arguments.ToImmutableArray();
            Result = result;
        }

        public override TypeSymbol Type => Symbol?.ValueType;

        public FunctionSymbol Symbol => Result.Selected?.Signature.Symbol;

        public ImmutableArray<BoundExpression> Arguments { get; }

        public OverloadResolutionResult<FunctionSymbolSignature> Result { get; }

        public BoundFunctionInvocationExpression Update(IEnumerable<BoundExpression> arguments, OverloadResolutionResult<FunctionSymbolSignature> result)
        {
            var newArguments = arguments.ToImmutableArray();

            if (newArguments == Arguments && result == Result)
                return this;

            return new BoundFunctionInvocationExpression(Syntax, newArguments, result);
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", Symbol.Name, string.Join(",", Arguments));
        }
    }
}