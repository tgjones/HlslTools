// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Classification
{
    internal abstract partial class AbstractClassificationService : IClassificationService
    {
        public abstract void AddSyntacticClassifications(SyntaxTreeBase syntaxTree, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken);

        public abstract void AddSemanticClassifications(SemanticModelBase semanticModel, TextSpan textSpan, Workspace workspace, List<ClassifiedSpan> result, CancellationToken cancellationToken);
    }
}
