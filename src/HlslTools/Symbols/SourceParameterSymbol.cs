using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class SourceParameterSymbol : ParameterSymbol
    {
        internal SourceParameterSymbol(ParameterSyntax syntax, Symbol parent, TypeSymbol valueType, ParameterDirection direction = ParameterDirection.In)
            : base(syntax.Declarator.Identifier.Text, string.Empty, parent, valueType, direction)
        {
            Syntax = syntax;
        }

        public ParameterSyntax Syntax { get; }

        public override bool HasDefaultValue => Syntax.Declarator.Initializer != null;
    }
}