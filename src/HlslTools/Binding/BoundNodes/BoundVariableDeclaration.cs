using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundVariableDeclaration : BoundStatement
    {
        public BoundVariableDeclaration(VariableDeclarationStatementSyntax syntax, LocalVariableSymbol variableSymbol, TypeSymbol declaredType, BoundExpression initializerOpt)
            : base(BoundNodeKind.VariableDeclaration, syntax)
        {
            VariableSymbol = variableSymbol;
            DeclaredType = declaredType;
            InitializerOpt = initializerOpt;
        }

        public LocalVariableSymbol VariableSymbol { get; }
        public TypeSymbol DeclaredType { get; }
        public BoundExpression InitializerOpt { get; }
    }
}