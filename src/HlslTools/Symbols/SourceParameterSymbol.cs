using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public class SourceParameterSymbol : ParameterSymbol
    {
        public SourceParameterSymbol(ParameterSyntax syntax, Symbol parent, TypeSymbol valueType, ParameterDirection direction = ParameterDirection.In)
            : base(syntax.Declarator.Identifier.Text, string.Empty, parent, valueType, direction)
        {
            Syntax = syntax;
        }

        public ParameterSyntax Syntax { get; }
    }
}