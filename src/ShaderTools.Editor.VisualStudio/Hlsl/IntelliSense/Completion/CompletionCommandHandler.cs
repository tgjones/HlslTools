using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.Editor.VisualStudio.Core.Util;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.Completion
{
    internal sealed class CompletionCommandHandler : CommandTargetBase<VSConstants.VSStd2KCmdID>
    {
        private readonly CompletionModelManager _completionModelManager;

        public CompletionCommandHandler(IVsTextView adapter, IWpfTextView textView, CompletionModelManager completionModelManager)
            : base(adapter, textView, 
                  VSConstants.VSStd2KCmdID.TYPECHAR, 
                  VSConstants.VSStd2KCmdID.RETURN, 
                  VSConstants.VSStd2KCmdID.TAB,
                  VSConstants.VSStd2KCmdID.BACKSPACE,
                  VSConstants.VSStd2KCmdID.DELETE)
        {
            _completionModelManager = completionModelManager;
        }

        public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return NextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            // Check for commit character.
            var typedChar = char.MinValue;
            if (pguidCmdGroup == CommandGroup && CommandIds.Contains(nCmdID))
            {
                if (nCmdID == (uint) VSConstants.VSStd2KCmdID.TYPECHAR)
                    typedChar = (char) (ushort) Marshal.GetObjectForNativeVariant(pvaIn);

                var isCommit = nCmdID == (uint) VSConstants.VSStd2KCmdID.RETURN
                    || nCmdID == (uint) VSConstants.VSStd2KCmdID.TAB;

                var isCompletion = isCommit
                    || char.IsWhiteSpace(typedChar) 
                    || (char.IsPunctuation(typedChar) && typedChar != '_');

                if (isCompletion && _completionModelManager.Commit())
                    if (isCommit)
                        return VSConstants.S_OK; // Don't add commit char to buffer.
            }

            // Pass command to next command target.
            var result = NextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            if (result == VSConstants.S_OK && pguidCmdGroup == CommandGroup && CommandIds.Contains(nCmdID))
            {
                var isTrigger = typedChar != char.MinValue && (char.IsLetterOrDigit(typedChar) || typedChar == '.');
                _completionModelManager.HandleTextInput(isTrigger);
            }

            return result;
        }

        protected override bool IsEnabled(VSConstants.VSStd2KCmdID commandId, ref string commandText)
        {
            throw new NotSupportedException();
        }

        protected override bool Execute(VSConstants.VSStd2KCmdID commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            throw new NotSupportedException();
        }
    }
}