namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract partial class SyntaxVisitor
    {
        public virtual void Visit(SyntaxNode node)
        {
            node?.Accept(this);
        }

        protected virtual void DefaultVisit(SyntaxNode node)
        {

        }

        public virtual void VisitSyntaxTrivia(SyntaxTrivia node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitTypeDeclarationStatement(TypeDeclarationStatementSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitTypedefStatement(TypedefStatementSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitTypeAlias(TypeAliasSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitStructType(StructTypeSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitTechnique(TechniqueSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitWhileStatement(WhileStatementSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSwitchSection(SwitchSectionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitPrefixCastExpression(CastExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitVectorType(VectorTypeSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSyntaxToken(SyntaxToken node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitObjectLikeMacroReference(ObjectLikeMacroReference node)
        {
            
        }

        public virtual void VisitFunctionLikeMacroReference(FunctionLikeMacroReference node)
        {

        }
    }

    public abstract partial class SyntaxVisitor<T>
    {
        public virtual T Visit(SyntaxNode node)
        {
            if (node != null)
                return node.Accept(this);
            return default(T);
        }

        protected virtual T DefaultVisit(SyntaxNode node)
        {
            return default(T);
        }

        public virtual T VisitSyntaxTrivia(SyntaxTrivia node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitTypeDeclarationStatement(TypeDeclarationStatementSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitTypedefStatement(TypedefStatementSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitTypeAlias(TypeAliasSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitStructType(StructTypeSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitTechnique(TechniqueSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitWhileStatement(WhileStatementSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitSwitchStatement(SwitchStatementSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitSwitchSection(SwitchSectionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitPrefixCastExpression(CastExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitVectorType(VectorTypeSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitSyntaxToken(SyntaxToken node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitObjectLikeMacroReference(ObjectLikeMacroReference node)
        {
            return default(T);
        }

        public virtual T VisitFunctionLikeMacroReference(FunctionLikeMacroReference node)
        {
            return default(T);
        }
    }
}