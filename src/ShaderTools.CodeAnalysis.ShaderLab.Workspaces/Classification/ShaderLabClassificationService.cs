// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Composition;
using System.Threading;
using ShaderTools.CodeAnalysis.Classification;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.ShaderLab.Syntax;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.ShaderLab.Classification
{
    [ExportLanguageService(typeof(IClassificationService), LanguageNames.ShaderLab), Shared]
    internal class ShaderLabClassificationService : AbstractClassificationService
    {
        //public override IEnumerable<ISyntaxClassifier> GetDefaultSyntaxClassifiers()
        //{
        //    return SyntaxClassifier.DefaultSyntaxClassifiers;
        //}

        //public override void AddLexicalClassifications(SourceText text, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken)
        //{
        //    ClassificationHelpers.AddLexicalClassifications(text, textSpan, result, cancellationToken);
        //}

        public override void AddSyntacticClassifications(SyntaxTreeBase syntaxTree, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken)
        {
            var worker = new SyntaxTaggerWorker(result, cancellationToken);
            worker.ClassifySyntax((SyntaxTree) syntaxTree);
        }

        //public override ClassifiedSpan FixClassification(SourceText rawText, ClassifiedSpan classifiedSpan)
        //{
        //    return ClassificationHelpers.AdjustStaleClassification(rawText, classifiedSpan);
        //}

        public override void AddSemanticClassifications(SemanticModelBase semanticModel, TextSpan textSpan, Workspace workspace, List<ClassifiedSpan> result, CancellationToken cancellationToken)
        {
            // TODO
        }
    }
}
