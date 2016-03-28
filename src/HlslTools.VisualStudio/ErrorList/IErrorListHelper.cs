using System;
using HlslTools.Diagnostics;
using HlslTools.Text;

namespace HlslTools.VisualStudio.ErrorList
{
    internal interface IErrorListHelper : IDisposable
    {
        void AddError(Diagnostic diagnostic, TextSpan span);
        void Clear();
    }
}