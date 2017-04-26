namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class PredefinedObjectTypeSyntax : PredefinedTypeSyntax
    {
        public readonly SyntaxToken ObjectTypeToken;
        public readonly TemplateArgumentListSyntax TemplateArgumentList;

        public PredefinedObjectTypeSyntax(SyntaxToken objectTypeToken, TemplateArgumentListSyntax templateArgumentList)
            : base(SyntaxKind.PredefinedObjectType)
        {
            RegisterChildNode(out ObjectTypeToken, objectTypeToken);
            RegisterChildNode(out TemplateArgumentList, templateArgumentList);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitPredefinedObjectType(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitPredefinedObjectType(this);
        }
    }
}