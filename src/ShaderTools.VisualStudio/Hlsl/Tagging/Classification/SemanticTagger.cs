using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.VisualStudio.Hlsl.Parsing;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Classification
{
    internal sealed class SemanticTagger : AsyncTagger<IClassificationTag>
    {
        private readonly HlslClassificationService _classificationService;

        public SemanticTagger(HlslClassificationService classificationService, BackgroundParser backgroundParser)
        {
            _classificationService = classificationService;

            backgroundParser.SubscribeToThrottledSemanticModelAvailable(BackgroundParserSubscriptionDelay.Short,
                async x => await InvalidateTags(x.Snapshot, x.CancellationToken));
        }

        protected override Tuple<ITextSnapshot, List<ITagSpan<IClassificationTag>>> GetTags(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            var semanticTags = new List<ITagSpan<IClassificationTag>>();

            SemanticModel semanticModel;
            if (snapshot.TryGetSemanticModel(cancellationToken, out semanticModel))
            {
                var semanticTaggerVisitor = new SemanticTaggerVisitor(semanticModel, _classificationService, snapshot, semanticTags, cancellationToken);
                semanticTaggerVisitor.VisitCompilationUnit((CompilationUnitSyntax) semanticModel.SyntaxTree.Root);
            }

            return Tuple.Create(snapshot, semanticTags);
        }
    }
}