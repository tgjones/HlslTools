using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class SourceFieldSymbol : FieldSymbol
    {
        internal SourceFieldSymbol(VariableDeclaratorSyntax syntax, TypeSymbol parent, TypeSymbol valueType)
            : base(syntax.Identifier.Text, string.Empty, parent, valueType)
        {
            Syntax = syntax;

            SourceTree = syntax.SyntaxTree;
            Locations = ImmutableArray.Create(Syntax.Identifier.SourceRange);
        }

        public VariableDeclaratorSyntax Syntax { get; }

        public override SyntaxTreeBase SourceTree { get; }
        public override ImmutableArray<SourceRange> Locations { get; }
    }
}