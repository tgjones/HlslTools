using System;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Text;

namespace ShaderTools.Editor.VisualStudio.Core.ErrorList
{
    internal interface IErrorListHelper : IDisposable
    {
        void AddError(DiagnosticBase diagnostic, TextSpan span);
        void Clear();
    }
}