using System.Collections.Immutable;
using System.Threading;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Structure;
using ShaderTools.CodeAnalysis.Text;

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
            if (startToken == null || !startToken.FileSpan.IsInRootFile
                || endToken == null || !endToken.FileSpan.IsInRootFile)
                return;

            var textSpan = new TextSpan(startToken.FileSpan.Span.End, endToken.FileSpan.Span.End - startToken.FileSpan.Span.End);
            var lineSpan = _sourceText.Lines.GetLinePositionSpan(textSpan);
            var isCollapsible = lineSpan.Start.Line != lineSpan.End.Line;

            _results.Add(new BlockSpan(isCollapsible, textSpan, autoCollapse: isImplementation));
        }
    }
}