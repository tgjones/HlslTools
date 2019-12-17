// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis;

namespace ShaderTools.VisualStudio.LanguageServices.Implementation.F1Help
{
    internal abstract class AbstractHelpContextService : IHelpContextService
    {
        public abstract string Language { get; }
        public abstract string Product { get; }

        public abstract Task<string> GetHelpTermAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken);
    }
}
