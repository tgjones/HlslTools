// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using ShaderTools.CodeAnalysis.Editor.Commands;

namespace ShaderTools.VisualStudio.LanguageServices.Implementation
{
    internal abstract partial class AbstractOleCommandTarget
    {
        public virtual int Exec(ref Guid pguidCmdGroup, uint commandId, uint executeInformation, IntPtr pvaIn, IntPtr pvaOut)
        {
            try
            {
                var subjectBuffer = GetSubjectBufferContainingCaret();
                this.CurrentlyExecutingCommand = commandId;

                // If we didn't get a subject buffer, then that means we're outside our code and we should ignore it
                // Also, ignore the command if executeInformation indicates isn't meant to be executed. From env\msenv\core\cmdwin.cpp:
                //      To query the parameter type list of a command, we call Exec with 
                //      the LOWORD of nCmdexecopt set to OLECMDEXECOPT_SHOWHELP (instead of
                //      the more usual OLECMDEXECOPT_DODEFAULT), the HIWORD of nCmdexecopt
                //      set to VSCmdOptQueryParameterList, pvaIn set to NULL, and pvaOut 
                //      pointing to a VARIANT ready to receive the result BSTR.
                var shouldSkipCommand = executeInformation == (((uint) VsMenus.VSCmdOptQueryParameterList << 16) | (uint) OLECMDEXECOPT.OLECMDEXECOPT_SHOWHELP);
                if (subjectBuffer == null || shouldSkipCommand)
                {
                    return NextCommandTarget.Exec(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
                }

                if (pguidCmdGroup == VSConstants.VSStd2K)
                {
                    return ExecuteVisualStudio2000(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
                }
                else
                {
                    return NextCommandTarget.Exec(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
                }
            }
            finally
            {
                this.CurrentlyExecutingCommand = default(uint);
            }
        }

        protected virtual int ExecuteVisualStudio2000(ref Guid pguidCmdGroup, uint commandId, uint executeInformation, IntPtr pvaIn, IntPtr pvaOut)
        {
            int result = VSConstants.S_OK;
            var guidCmdGroup = pguidCmdGroup;
            Action executeNextCommandTarget = () =>
            {
                result = NextCommandTarget.Exec(ref guidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
            };

            switch ((VSConstants.VSStd2KCmdID) commandId)
            {
                case VSConstants.VSStd2KCmdID.OPENFILE:
                    ExecuteOpenFile(executeNextCommandTarget);
                    break;

                default:
                    return NextCommandTarget.Exec(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
            }

            return result;
        }

        private void ExecuteOpenFile(Action executeNextCommandTarget)
        {
            CurrentHandlers.Execute(
                (textView, textBuffer) => new OpenFileCommandArgs(ConvertTextView(), textBuffer),
                executeNextCommandTarget);
        }
    }
}
