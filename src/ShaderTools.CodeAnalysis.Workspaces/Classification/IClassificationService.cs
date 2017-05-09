// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Classification
{
    internal interface IClassificationService : ILanguageService
    {
        //IEnumerable<ISyntaxClassifier> GetDefaultSyntaxClassifiers();

        //void AddLexicalClassifications(SourceText text,
        //    TextSpan textSpan,
        //    List<ClassifiedSpan> result,
        //    CancellationToken cancellationToken);

        void AddSyntacticClassifications(SyntaxTreeBase syntaxTree,
            TextSpan textSpan,
            List<ClassifiedSpan> result,
            CancellationToken cancellationToken);

        //Task AddSemanticClassificationsAsync(Document document,
        //    TextSpan textSpan,
        //    Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers,
        //    Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers,
        //    List<ClassifiedSpan> result,
        //    CancellationToken cancellationToken);

        //void AddSemanticClassifications(
        //    SemanticModel semanticModel,
        //    TextSpan textSpan,
        //    Workspace workspace,
        //    Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers,
        //    Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers,
        //    List<ClassifiedSpan> result,
        //    CancellationToken cancellationToken);

        void AddSemanticClassifications(
            SemanticModelBase semanticModel,
            TextSpan textSpan,
            Workspace workspace,
            List<ClassifiedSpan> result,
            CancellationToken cancellationToken);

        //ClassifiedSpan FixClassification(SourceText text, ClassifiedSpan classifiedSpan);
    }
}
