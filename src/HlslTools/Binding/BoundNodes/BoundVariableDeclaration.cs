using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundVariableDeclaration : BoundStatement
    {
        public BoundVariableDeclaration(VariableSymbol variableSymbol, TypeSymbol declaredType, BoundExpression initializerOpt)
            : base(BoundNodeKind.VariableDeclaration)
        {
            VariableSymbol = variableSymbol;
            DeclaredType = declaredType;
            InitializerOpt = initializerOpt;
        }

        public VariableSymbol VariableSymbol { get; }
        public TypeSymbol DeclaredType { get; }
        public BoundExpression InitializerOpt { get; }
    }
}