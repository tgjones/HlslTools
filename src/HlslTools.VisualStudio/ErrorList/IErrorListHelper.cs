using HlslTools.Diagnostics;
using HlslTools.Text;

namespace HlslTools.VisualStudio.ErrorList
{
    internal interface IErrorListHelper
    {
        void AddError(Diagnostic diagnostic, TextSpan span);
        void Clear();
    }
}