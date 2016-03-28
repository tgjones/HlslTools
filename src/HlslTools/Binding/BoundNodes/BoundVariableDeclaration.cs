using System.Collections.Immutable;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundVariableDeclaration : BoundStatement
    {
        public BoundVariableDeclaration(VariableSymbol variableSymbol, TypeSymbol declaredType, ImmutableArray<BoundVariableQualifier> qualifiers, BoundInitializer initializerOpt)
            : base(BoundNodeKind.VariableDeclaration)
        {
            VariableSymbol = variableSymbol;
            DeclaredType = declaredType;
            Qualifiers = qualifiers;
            InitializerOpt = initializerOpt;
        }

        public VariableSymbol VariableSymbol { get; }
        public TypeSymbol DeclaredType { get; }
        public ImmutableArray<BoundVariableQualifier> Qualifiers { get; }
        public BoundInitializer InitializerOpt { get; }
    }
}