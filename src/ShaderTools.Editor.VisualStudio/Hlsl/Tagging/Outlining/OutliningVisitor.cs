using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Outlining
{
    internal sealed class OutliningVisitor : SyntaxWalker
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

        protected override void DefaultVisit(SyntaxNode node)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            base.DefaultVisit(node);
        }

        public override void VisitNamespace(NamespaceSyntax node)
        {
            CreateTag(node.Name, node.CloseBraceToken, false);
            base.VisitNamespace(node);
        }

        public override void VisitTypeDeclarationStatement(TypeDeclarationStatementSyntax node)
        {
            CreateTag(node.Type.NameToken, node.SemicolonToken, false);
            base.VisitTypeDeclarationStatement(node);
        }

        public override void VisitConstantBuffer(ConstantBufferSyntax node)
        {
            CreateTag(node.Name, node.CloseBraceToken, false);
            base.VisitConstantBuffer(node);
        }

        public override void VisitFunctionDefinition(FunctionDefinitionSyntax node)
        {
            CreateTag(node.ParameterList.CloseParenToken, node.Body.CloseBraceToken, true);
            base.VisitFunctionDefinition(node);
        }

        public override void VisitTechnique(TechniqueSyntax node)
        {
            CreateTag(node.Name, node.CloseBraceToken, false);
            base.VisitTechnique(node);
        }

        private void CreateTag(SyntaxToken startToken, SyntaxToken endToken, bool isImplementation)
        {
            if (startToken == null || !startToken.Span.IsInRootFile
                || endToken == null || !endToken.Span.IsInRootFile)
                return;

            var span = new Span(startToken.Span.End, endToken.Span.End - startToken.Span.End);
            if (_snapshot.GetLineNumberFromPosition(span.Start) == _snapshot.GetLineNumberFromPosition(span.End))
                return;

            var snapshotSpan = new SnapshotSpan(_snapshot, span);
            var tag = new OutliningRegionTag(false, isImplementation, "...", snapshotSpan.GetText());
            var tagSpan = new TagSpan<IOutliningRegionTag>(snapshotSpan, tag);

            _results.Add(tagSpan);
        }
    }
}