using System;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.Editor.VisualStudio.Core.ErrorList
{
    internal interface IErrorListHelper : IDisposable
    {
        void AddError(SyntaxTreeBase syntaxTree, Diagnostic diagnostic);
        void Clear();
    }
}