using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed partial class StructTypeSyntax : TypeDefinitionSyntax
    {
        public bool IsClass => Kind == SyntaxKind.ClassType;

        public StructTypeSyntax(SyntaxToken structKeyword, SyntaxToken name, BaseListSyntax baseList, SyntaxToken openBraceToken, List<SyntaxNode> members, SyntaxToken closeBraceToken)
            : this(structKeyword.Kind == SyntaxKind.ClassKeyword ? SyntaxKind.ClassType : SyntaxKind.StructType,
                   structKeyword, name, baseList, openBraceToken, members, closeBraceToken)
        {
        }
    }
}