using HlslTools.Diagnostics;
using HlslTools.Text;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.ErrorList
{
    internal interface IErrorListHelper
    {
        void AddError(ITextSnapshot snapshot, Diagnostic diagnostic, TextSpan span);
        void Clear();
    }
}