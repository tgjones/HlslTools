// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
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

                var contentType = subjectBuffer.ContentType;

                if (pguidCmdGroup == VSConstants.VSStd2K)
                {
                    return ExecuteVisualStudio2000(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut, subjectBuffer, contentType);
                }
                else if (pguidCmdGroup == VSConstants.GUID_AppCommand)
                {
                    return ExecuteAppCommand(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut, subjectBuffer, contentType);
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

        private int ExecuteAppCommand(ref Guid pguidCmdGroup, uint commandId, uint executeInformation, IntPtr pvaIn, IntPtr pvaOut, ITextBuffer subjectBuffer, IContentType contentType)
        {
            int result = VSConstants.S_OK;
            var guidCmdGroup = pguidCmdGroup;
            Action executeNextCommandTarget = () =>
            {
                result = NextCommandTarget.Exec(ref guidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
            };

            switch ((VSConstants.AppCommandCmdID) commandId)
            {
                case VSConstants.AppCommandCmdID.BrowserBackward:
                    ExecuteBrowserBackward(subjectBuffer, contentType, executeNextCommandTarget);
                    break;

                case VSConstants.AppCommandCmdID.BrowserForward:
                    ExecuteBrowserForward(subjectBuffer, contentType, executeNextCommandTarget);
                    break;

                default:
                    return NextCommandTarget.Exec(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
            }

            return result;
        }

        protected virtual int ExecuteVisualStudio2000(ref Guid pguidCmdGroup, uint commandId, uint executeInformation, IntPtr pvaIn, IntPtr pvaOut, ITextBuffer subjectBuffer, IContentType contentType)
        {
            int result = VSConstants.S_OK;
            var guidCmdGroup = pguidCmdGroup;
            Action executeNextCommandTarget = () =>
            {
                result = NextCommandTarget.Exec(ref guidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
            };

            switch ((VSConstants.VSStd2KCmdID) commandId)
            {
                case VSConstants.VSStd2KCmdID.CANCEL:
                    ExecuteCancel(subjectBuffer, contentType, executeNextCommandTarget);
                    break;

                case CmdidNextHighlightedReference:
                    ExecuteNextHighlightedReference(subjectBuffer, contentType, executeNextCommandTarget);
                    break;

                case CmdidPreviousHighlightedReference:
                    ExecutePreviousHighlightedReference(subjectBuffer, contentType, executeNextCommandTarget);
                    break;

                case VSConstants.VSStd2KCmdID.QUICKINFO:
                    GCManager.UseLowLatencyModeForProcessingUserInput();
                    ExecuteQuickInfo(subjectBuffer, contentType, executeNextCommandTarget);
                    break;

                default:
                    return NextCommandTarget.Exec(ref pguidCmdGroup, commandId, executeInformation, pvaIn, pvaOut);
            }

            return result;
        }

        protected void ExecuteQuickInfo(ITextBuffer subjectBuffer, IContentType contentType, Action executeNextCommandTarget)
        {
            CurrentHandlers.Execute(contentType,
                args: new InvokeQuickInfoCommandArgs(ConvertTextView(), subjectBuffer),
                lastHandler: executeNextCommandTarget);
        }

        protected void ExecutePreviousHighlightedReference(ITextBuffer subjectBuffer, IContentType contentType, Action executeNextCommandTarget)
        {
            CurrentHandlers.Execute(contentType,
                args: new NavigateToHighlightedReferenceCommandArgs(ConvertTextView(), subjectBuffer, NavigateDirection.Up),
                lastHandler: executeNextCommandTarget);
        }

        protected void ExecuteNextHighlightedReference(ITextBuffer subjectBuffer, IContentType contentType, Action executeNextCommandTarget)
        {
            CurrentHandlers.Execute(contentType,
                args: new NavigateToHighlightedReferenceCommandArgs(ConvertTextView(), subjectBuffer, NavigateDirection.Down),
                lastHandler: executeNextCommandTarget);
        }

        protected void ExecuteCancel(ITextBuffer subjectBuffer, IContentType contentType, Action executeNextCommandTarget)
        {
            CurrentHandlers.Execute(contentType,
                args: new EscapeKeyCommandArgs(ConvertTextView(), subjectBuffer),
                lastHandler: executeNextCommandTarget);
        }

        private void ExecuteBrowserBackward(ITextBuffer subjectBuffer, IContentType contentType, Action executeNextCommandTarget)
        {
            ExecuteBrowserNavigationCommand(navigateBackward: true, executeNextCommandTarget: executeNextCommandTarget);
        }

        private void ExecuteBrowserForward(ITextBuffer subjectBuffer, IContentType contentType, Action executeNextCommandTarget)
        {
            ExecuteBrowserNavigationCommand(navigateBackward: false, executeNextCommandTarget: executeNextCommandTarget);
        }

        private void ExecuteBrowserNavigationCommand(bool navigateBackward, Action executeNextCommandTarget)
        {
            // We just want to delegate to the shell's NavigateBackward/Forward commands
            var target = _serviceProvider.GetService(typeof(SUIHostCommandDispatcher)) as IOleCommandTarget;
            if (target != null)
            {
                var cmd = (uint) (navigateBackward ?
                     VSConstants.VSStd97CmdID.ShellNavBackward :
                     VSConstants.VSStd97CmdID.ShellNavForward);

                OLECMD[] cmds = new[] { new OLECMD() { cmdf = 0, cmdID = cmd } };
                var hr = target.QueryStatus(VSConstants.GUID_VSStandardCommandSet97, 1, cmds, IntPtr.Zero);
                if (hr == VSConstants.S_OK && (cmds[0].cmdf & (uint) OLECMDF.OLECMDF_ENABLED) != 0)
                {
                    // ignore failure
                    target.Exec(VSConstants.GUID_VSStandardCommandSet97, cmd, (uint) OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
                    return;
                }
            }

            executeNextCommandTarget();
        }
    }
}
