using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HlslTools.Syntax;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging.Classification
{
    internal sealed class SemanticTaggerVisitor : SyntaxWalker
    {
        private readonly HlslClassificationService _classificationService;
        private readonly ITextSnapshot _snapshot;
        private readonly List<ITagSpan<IClassificationTag>> _results;
        private readonly CancellationToken _cancellationToken;

        public SemanticTaggerVisitor(HlslClassificationService classificationService, ITextSnapshot snapshot, List<ITagSpan<IClassificationTag>> results, CancellationToken cancellationToken)
        {
            _classificationService = classificationService;
            _snapshot = snapshot;
            _results = results;
            _cancellationToken = cancellationToken;
        }

        public override void Visit(SyntaxNode node)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            base.Visit(node);
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            CreateTag(node.Name.Name, _classificationService.ClassIdentifier);
            base.VisitAttribute(node);
        }

        public override void VisitClassType(ClassTypeSyntax node)
        {
            CreateTag(node.Name, _classificationService.ClassIdentifier);

            base.VisitClassType(node);
        }

        public override void VisitInterfaceType(InterfaceTypeSyntax node)
        {
            CreateTag(node.Name, _classificationService.ClassIdentifier);

            base.VisitInterfaceType(node);
        }

        public override void VisitStructType(StructTypeSyntax node)
        {
            CreateTag(node.Name, _classificationService.ClassIdentifier);

            base.VisitStructType(node);
        }

        public override void VisitConstantBuffer(ConstantBufferSyntax node)
        {
            CreateTag(node.Name, _classificationService.ClassIdentifier);

            base.VisitConstantBuffer(node);
        }

        public override void VisitFunctionDefinition(FunctionDefinitionSyntax node)
        {
            CreateTag(node.Name.GetUnqualifiedName().Name, _classificationService.FunctionIdentifier);

            base.VisitFunctionDefinition(node);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            if (node.Ancestors().Any(x => x is FunctionDefinitionSyntax))
                CreateTag(node.Identifier, _classificationService.LocalVariableIdentifier);
            else if (node.Ancestors().Any(x => x is TypeDefinitionSyntax))
                CreateTag(node.Identifier, _classificationService.FieldIdentifier);
            else
                CreateTag(node.Identifier, _classificationService.GlobalVariableIdentifier);

            base.VisitVariableDeclarator(node);
        }

        public override void VisitSemantic(SemanticSyntax node)
        {
            CreateTag(node.Semantic, _classificationService.Semantic);
            base.VisitSemantic(node);
        }

        public override void VisitRegisterLocation(RegisterLocation node)
        {
            CreateTag(node.Register, _classificationService.RegisterLocation);
            base.VisitRegisterLocation(node);
        }

        public override void VisitLogicalRegisterSpace(LogicalRegisterSpace node)
        {
            CreateTag(node.SpaceToken, _classificationService.RegisterLocation);
            base.VisitLogicalRegisterSpace(node);
        }

        public override void VisitPackOffsetLocation(PackOffsetLocation node)
        {
            CreateTag(node.Register, _classificationService.PackOffset);
            if (node.ComponentPart != null)
            {
                CreateTag(node.ComponentPart.DotToken, _classificationService.PackOffset);
                CreateTag(node.ComponentPart.Component, _classificationService.PackOffset);
            }
            base.VisitPackOffsetLocation(node);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            // TODO: Figure out what type of identifier this is.
            base.VisitIdentifierName(node);
        }

        private void CreateTag(SyntaxToken token, IClassificationType classificationType)
        {
            if (token == null || !token.Span.IsInRootFile)
                return;

            var snapshotSpan = new SnapshotSpan(_snapshot, token.Span.Start, token.Span.Length);
            var tag = new ClassificationTag(classificationType);
            var tagSpan = new TagSpan<IClassificationTag>(snapshotSpan, tag);

            _results.Add(tagSpan);
        }
    }
}