using System;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.VisualStudio.Hlsl.ErrorList
{
    internal interface IErrorListHelper : IDisposable
    {
        void AddError(Diagnostic diagnostic, TextSpan span);
        void Clear();
    }
}