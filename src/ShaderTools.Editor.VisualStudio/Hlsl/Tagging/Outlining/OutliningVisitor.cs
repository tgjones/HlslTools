using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Outlining
{
    internal sealed class OutliningVisitor : SyntaxVisitor
    {
        private readonly ITextSnapshot _snapshot;
        private readonly List<ITagSpan<IOutliningRegionTag>> _results;
        private readonly CancellationToken _cancellationToken;

        public OutliningVisitor(ITextSnapshot snapshot, List<ITagSpan<IOutliningRegionTag>> results, CancellationToken cancellationToken)
        {
            _snapshot = snapshot;
            _results = results;
            _cancellationToken = cancellationToken;
        }

        public override void Visit(SyntaxNode node)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            base.Visit(node);
        }

        public override void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            foreach (var childNode in node.ChildNodes)
                Visit((SyntaxNode) childNode);
        }

        public override void VisitTypeDeclarationStatement(TypeDeclarationStatementSyntax node)
        {
            CreateTag(node.Type.NameToken, node.SemicolonToken);
        }

        public override void VisitConstantBuffer(ConstantBufferSyntax node)
        {
            CreateTag(node.Name, node.CloseBraceToken);
        }

        public override void VisitFunctionDefinition(FunctionDefinitionSyntax node)
        {
            CreateTag(node.ParameterList.CloseParenToken, node.Body.CloseBraceToken);
        }

        public override void VisitTechnique(TechniqueSyntax node)
        {
            CreateTag(node.Name, node.CloseBraceToken);
        }

        private void CreateTag(SyntaxToken startToken, SyntaxToken endToken)
        {
            if (startToken == null || !startToken.Span.IsInRootFile
                || endToken == null || !endToken.Span.IsInRootFile)
                return;

            var span = new Span(startToken.Span.End, endToken.Span.End - startToken.Span.End);
            if (_snapshot.GetLineNumberFromPosition(span.Start) == _snapshot.GetLineNumberFromPosition(span.End))
                return;

            var snapshotSpan = new SnapshotSpan(_snapshot, span);
            var tag = new OutliningRegionTag("...", snapshotSpan.GetText());
            var tagSpan = new TagSpan<IOutliningRegionTag>(snapshotSpan, tag);

            _results.Add(tagSpan);
        }
    }
}