using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Structure;

namespace ShaderTools.CodeAnalysis.Hlsl.Structure
{
    internal sealed class OutliningVisitor : SyntaxWalker
    {
        private readonly SourceText _sourceText;
        private readonly ImmutableArray<BlockSpan>.Builder _results;
        private readonly CancellationToken _cancellationToken;

        public OutliningVisitor(SourceText sourceText, ImmutableArray<BlockSpan>.Builder results, CancellationToken cancellationToken)
        {
            _sourceText = sourceText;
            _results = results;
            _cancellationToken = cancellationToken;
        }

        protected override void DefaultVisit(SyntaxNode node)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            base.DefaultVisit(node);
        }

        public override void VisitNamespace(NamespaceSyntax node)
        {
            CreateTag(
                BlockSpanType.Namespace, 
                node.Name, 
                node.CloseBraceToken, 
                node.NamespaceKeyword,
                node.Name,
                false);
            base.VisitNamespace(node);
        }

        public override void VisitTypeDeclarationStatement(TypeDeclarationStatementSyntax node)
        {
            SyntaxToken hintStartToken;
            switch (node.Type)
            {
                case InterfaceTypeSyntax i:
                    hintStartToken = i.InterfaceKeyword;
                    break;
                case StructTypeSyntax s:
                    hintStartToken = s.StructKeyword;
                    break;
                default:
                    throw new System.InvalidOperationException();
            }

            CreateTag(
                BlockSpanType.Type, 
                node.Type.NameToken, 
                node.SemicolonToken,
                hintStartToken,
                node.Type.NameToken,
                false);
            base.VisitTypeDeclarationStatement(node);
        }

        public override void VisitConstantBuffer(ConstantBufferSyntax node)
        {
            CreateTag(
                BlockSpanType.Type, 
                node.Name, 
                node.CloseBraceToken,
                node.ConstantBufferKeyword,
                node.Name,
                false);
            base.VisitConstantBuffer(node);
        }

        public override void VisitFunctionDefinition(FunctionDefinitionSyntax node)
        {
            CreateTag(
                BlockSpanType.Member, 
                node.ParameterList.CloseParenToken, 
                node.Body.CloseBraceToken, 
                node.ReturnType.GetFirstTokenInDescendants(),
                node.ParameterList.CloseParenToken,
                true);
            base.VisitFunctionDefinition(node);
        }

        public override void VisitTechnique(TechniqueSyntax node)
        {
            CreateTag(
                BlockSpanType.Type, 
                node.Name, 
                node.CloseBraceToken,
                node.TechniqueKeyword,
                node.Name,
                false);
            base.VisitTechnique(node);
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            if (node.Statement is BlockSyntax b)
            {
                CreateTag(
                    BlockSpanType.Conditional,
                    node.CloseParenToken,
                    b.CloseBraceToken,
                    node.IfKeyword,
                    node.CloseParenToken,
                    false);
            }
            base.VisitIfStatement(node);
        }

        public override void VisitElseClause(ElseClauseSyntax node)
        {
            if (node.Statement is BlockSyntax b)
            {
                CreateTag(
                    BlockSpanType.Conditional,
                    node.ElseKeyword, 
                    b.CloseBraceToken, 
                    node.ElseKeyword,
                    node.ElseKeyword,
                    false);
            }
            base.VisitElseClause(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            if (node.Statement is BlockSyntax b)
            {
                CreateTag(
                    BlockSpanType.Loop,
                    node.CloseParenToken, 
                    b.CloseBraceToken, 
                    node.ForKeyword,
                    node.CloseParenToken,
                    false);
            }
            base.VisitForStatement(node);
        }

        public override void VisitSyntaxToken(SyntaxToken node)
        {
            foreach (var trivia in node.LeadingTrivia)
                Visit(trivia);

            foreach (var trivia in node.TrailingTrivia)
                Visit(trivia);

            base.VisitSyntaxToken(node);
        }

        public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
        {
            CreateBranchingDirectiveTag(node);
        }

        public override void VisitIfDefDirectiveTrivia(IfDefDirectiveTriviaSyntax node)
        {
            CreateBranchingDirectiveTag(node);
        }

        public override void VisitIfNDefDirectiveTrivia(IfNDefDirectiveTriviaSyntax node)
        {
            CreateBranchingDirectiveTag(node);
        }

        private void CreateBranchingDirectiveTag(BranchingDirectiveTriviaSyntax node)
        {
            if (!node.BranchTaken)
            {
                return;
            }

            if (node.BranchEnd == null)
            {
                return;
            }

            CreateTag(
                BlockSpanType.PreprocessorRegion,
                node.EndOfDirectiveToken,
                node.BranchEnd.EndOfDirectiveToken,
                node.HashToken,
                node.EndOfDirectiveToken,
                false);
        }

        private void CreateTag(
            BlockSpanType type, 
            SyntaxToken startToken, 
            SyntaxToken endToken, 
            SyntaxToken hintStartToken,
            SyntaxToken hintEndToken,
            bool isImplementation)
        {
            if (startToken == null || !startToken.FileSpan.IsInRootFile
                || endToken == null || !endToken.FileSpan.IsInRootFile)
                return;

            var textSpan = new TextSpan(startToken.FileSpan.Span.End, endToken.FileSpan.Span.End - startToken.FileSpan.Span.End);
            var lineSpan = _sourceText.Lines.GetLinePositionSpan(textSpan);
            var isCollapsible = lineSpan.Start.Line != lineSpan.End.Line;

            _results.Add(new BlockSpan(
                type, 
                isCollapsible, 
                textSpan, 
                TextSpan.FromBounds(hintStartToken.FileSpan.Span.Start, hintEndToken.FileSpan.Span.End),
                "...",
                isImplementation,
                false));
        }
    }
}