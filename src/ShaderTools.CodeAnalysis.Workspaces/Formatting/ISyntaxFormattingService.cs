// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Formatting
{
    internal interface ISyntaxFormattingService : ILanguageService
    {
        string Format(SyntaxTreeBase tree, TextSpan span, OptionSet options, CancellationToken cancellationToken);
    }
}
