// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;
using ShaderTools.CodeAnalysis.Editor;
using ShaderTools.CodeAnalysis.Editor.Commands;
using ShaderTools.Utilities.Diagnostics;

namespace ShaderTools.VisualStudio.LanguageServices.Implementation
{
    internal abstract partial class AbstractOleCommandTarget
    {
        private const int ECMD_SMARTTASKS = 147;

        public int QueryStatus(ref Guid pguidCmdGroup, uint commandCount, OLECMD[] prgCmds, IntPtr commandText)
        {
            Contract.ThrowIfFalse(commandCount == 1);
            Contract.ThrowIfFalse(prgCmds.Length == 1);

            // TODO: We'll need to extend the command handler interfaces at some point when we have commands that
            // require enabling/disabling at some point.  For now, we just enable the few that we care about.
            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                return QueryVisualStudio2000Status(ref pguidCmdGroup, commandCount, prgCmds, commandText);
            }
            else
            {
                return NextCommandTarget.QueryStatus(ref pguidCmdGroup, commandCount, prgCmds, commandText);
            }
        }

        private int QueryVisualStudio2000Status(ref Guid pguidCmdGroup, uint commandCount, OLECMD[] prgCmds, IntPtr commandText)
        {
            switch ((VSConstants.VSStd2KCmdID) prgCmds[0].cmdID)
            {
                case VSConstants.VSStd2KCmdID.OPENFILE:
                    return QueryOpenFileStatus(ref pguidCmdGroup, commandCount, prgCmds, commandText);

                default:
                    return NextCommandTarget.QueryStatus(ref pguidCmdGroup, commandCount, prgCmds, commandText);
            }
        }

        private int GetCommandState<T>(
            Func<ITextView, ITextBuffer, T> createArgs,
            ref Guid pguidCmdGroup,
            uint commandCount,
            OLECMD[] prgCmds,
            IntPtr commandText)
            where T : EditorCommandArgs
        {
            var result = VSConstants.S_OK;

            var guidCmdGroup = pguidCmdGroup;
            Func<CommandState> executeNextCommandTarget = () =>
            {
                result = NextCommandTarget.QueryStatus(ref guidCmdGroup, commandCount, prgCmds, commandText);

                var isAvailable = ((OLECMDF) prgCmds[0].cmdf & OLECMDF.OLECMDF_ENABLED) == OLECMDF.OLECMDF_ENABLED;
                var isChecked = ((OLECMDF) prgCmds[0].cmdf & OLECMDF.OLECMDF_LATCHED) == OLECMDF.OLECMDF_LATCHED;
                return new CommandState(isAvailable, isChecked, GetText(commandText));
            };

            CommandState commandState;
            var subjectBuffer = GetSubjectBufferContainingCaret();
            if (subjectBuffer == null)
            {
                commandState = executeNextCommandTarget();
            }
            else
            {
                commandState = CurrentHandlers.GetCommandState<T>(
                    (textView, textBuffer) => createArgs(ConvertTextView(), subjectBuffer),
                    executeNextCommandTarget);
            }

            var enabled = commandState.IsAvailable ? OLECMDF.OLECMDF_ENABLED : OLECMDF.OLECMDF_INVISIBLE;
            var latched = commandState.IsChecked ? OLECMDF.OLECMDF_LATCHED : OLECMDF.OLECMDF_NINCHED;

            prgCmds[0].cmdf = (uint) (enabled | latched | OLECMDF.OLECMDF_SUPPORTED);

            if (!string.IsNullOrEmpty(commandState.DisplayText) && GetText(commandText) != commandState.DisplayText)
            {
                SetText(commandText, commandState.DisplayText);
            }

            return result;
        }

        private int QueryOpenFileStatus(ref Guid pguidCmdGroup, uint commandCount, OLECMD[] prgCmds, IntPtr commandText)
        {
            return GetCommandState(
                (v, b) => new OpenFileCommandArgs(v, b),
                ref pguidCmdGroup, commandCount, prgCmds, commandText);
        }

        private static unsafe string GetText(IntPtr pCmdTextInt)
        {
            if (pCmdTextInt == IntPtr.Zero)
            {
                return string.Empty;
            }

            OLECMDTEXT* pText = (OLECMDTEXT*) pCmdTextInt;

            // Punt early if there is no text in the structure.
            if (pText->cwActual == 0)
            {
                return string.Empty;
            }

            return new string((char*) &pText->rgwz, 0, (int) pText->cwActual);
        }

        private static unsafe void SetText(IntPtr pCmdTextInt, string text)
        {
            OLECMDTEXT* pText = (OLECMDTEXT*) pCmdTextInt;

            // If, for some reason, we don't get passed an array, we should just bail
            if (pText->cwBuf == 0)
            {
                return;
            }

            fixed (char* pinnedText = text)
            {
                char* src = pinnedText;
                char* dest = (char*) (&pText->rgwz);

                // Don't copy too much, and make sure to reserve space for the terminator
                int length = Math.Min(text.Length, (int) pText->cwBuf - 1);

                for (int i = 0; i < length; i++)
                {
                    *dest++ = *src++;
                }

                // Add terminating NUL
                *dest = '\0';

                pText->cwActual = (uint) length;
            }
        }
    }
}
