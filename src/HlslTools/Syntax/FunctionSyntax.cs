using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public abstract class FunctionSyntax : SyntaxNode
    {
        public readonly List<AttributeSyntax> Attributes;
        public readonly List<SyntaxToken> Modifiers;
        public readonly TypeSyntax ReturnType;
        public readonly DeclarationNameSyntax Name;
        public readonly ParameterListSyntax ParameterList;
        public readonly SemanticSyntax Semantic;

        protected FunctionSyntax(SyntaxKind kind, List<AttributeSyntax> attributes, List<SyntaxToken> modifiers, TypeSyntax returnType, DeclarationNameSyntax name, ParameterListSyntax parameterList, SemanticSyntax semantic)
            : base(kind)
        {
            RegisterChildNodes(out Attributes, attributes);
            RegisterChildNodes(out Modifiers, modifiers);
            RegisterChildNode(out ReturnType, returnType);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out ParameterList, parameterList);
            RegisterChildNode(out Semantic, semantic);
        }
    }
}