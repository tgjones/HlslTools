using System;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Core.ErrorList
{
    internal interface IErrorListHelper : IDisposable
    {
        void AddError(Diagnostic diagnostic, TextSpan span);
        void Clear();
    }
}