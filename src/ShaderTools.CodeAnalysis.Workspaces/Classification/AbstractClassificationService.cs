// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Classification
{
    internal abstract partial class AbstractClassificationService : IClassificationService
    {
        protected AbstractClassificationService()
        {
        }

        //public abstract void AddLexicalClassifications(SourceText text, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken);
        public abstract void AddSyntacticClassifications(SyntaxTreeBase syntaxTree, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken);

        //public abstract IEnumerable<ISyntaxClassifier> GetDefaultSyntaxClassifiers();
        //public abstract ClassifiedSpan FixClassification(SourceText text, ClassifiedSpan classifiedSpan);

        //public async Task AddSemanticClassificationsAsync(Document document, TextSpan textSpan, Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers, Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers, List<ClassifiedSpan> result, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var semanticModel = await document.GetSemanticModelForSpanAsync(textSpan, cancellationToken).ConfigureAwait(false);
        //        AddSemanticClassifications(semanticModel, textSpan, document.Project.Solution.Workspace, getNodeClassifiers, getTokenClassifiers, result, cancellationToken);
        //    }
        //    catch (Exception e) when (FatalError.ReportUnlessCanceled(e))
        //    {
        //        throw ExceptionUtilities.Unreachable;
        //    }
        //}

        //public void AddSemanticClassifications(SemanticModel semanticModel, TextSpan textSpan, Workspace workspace, Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers, Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers, List<ClassifiedSpan> result, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    Worker.Classify(workspace, semanticModel, textSpan, result, getNodeClassifiers, getTokenClassifiers, cancellationToken);
        //}

        public abstract void AddSemanticClassifications(SemanticModelBase semanticModel, TextSpan textSpan, Workspace workspace, List<ClassifiedSpan> result, CancellationToken cancellationToken);
    }
}
