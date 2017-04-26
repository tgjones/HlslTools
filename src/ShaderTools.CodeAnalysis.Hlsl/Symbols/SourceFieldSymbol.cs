using ShaderTools.CodeAnalysis.Hlsl.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class SourceFieldSymbol : FieldSymbol
    {
        internal SourceFieldSymbol(VariableDeclaratorSyntax syntax, TypeSymbol parent, TypeSymbol valueType)
            : base(syntax.Identifier.Text, string.Empty, parent, valueType)
        {
            Syntax = syntax;
        }

        public VariableDeclaratorSyntax Syntax { get; }
    }
}