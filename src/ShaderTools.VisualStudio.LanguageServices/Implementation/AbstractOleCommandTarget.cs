// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.Utilities.Diagnostics;

namespace ShaderTools.VisualStudio.LanguageServices.Implementation
{
    internal abstract partial class AbstractOleCommandTarget : IOleCommandTarget
    {
        private readonly IEditorCommandHandlerServiceFactory _commandHandlerServiceFactory;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactory;

        /// <summary>
        /// This is set only during Exec. Currently, this is required to disambiguate the editor calls to
        /// <see cref="IVsTextViewFilter.GetPairExtents(int, int, TextSpan[])"/> between GotoBrace and GotoBraceExt commands.
        /// </summary>
        protected uint CurrentlyExecutingCommand { get; private set; }

        public AbstractOleCommandTarget(
            IWpfTextView wpfTextView,
            IEditorCommandHandlerServiceFactory commandHandlerServiceFactory,
            IVsEditorAdaptersFactoryService editorAdaptersFactory)
        {
            Contract.ThrowIfNull(wpfTextView);
            Contract.ThrowIfNull(commandHandlerServiceFactory);
            Contract.ThrowIfNull(editorAdaptersFactory);

            WpfTextView = wpfTextView;
            _commandHandlerServiceFactory = commandHandlerServiceFactory;
            _editorAdaptersFactory = editorAdaptersFactory;
        }

        /// <summary>
        /// The IWpfTextView that this command filter is attached to.
        /// </summary>
        public IWpfTextView WpfTextView { get; }

        /// <summary>
        /// The command handler service to use for dispatching commands. This is set by
        /// the derived classes to this class.
        /// </summary>
        protected IEditorCommandHandlerService CurrentHandlers { get; set; }

        /// <summary>
        /// The next command target in the chain. This is set by the derived implementation of this
        /// class.
        /// </summary>
        protected internal IOleCommandTarget NextCommandTarget { get; set; }

        protected internal void RefreshCommandFilters()
        {
            this.CurrentHandlers = _commandHandlerServiceFactory.GetService(WpfTextView);
        }

        internal AbstractOleCommandTarget AttachToVsTextView()
        {
            var vsTextView = _editorAdaptersFactory.GetViewAdapter(WpfTextView);
            // Add command filter to IVsTextView. If something goes wrong, throw.
            int returnValue = vsTextView.AddCommandFilter(this, out var nextCommandTarget);
            Marshal.ThrowExceptionForHR(returnValue);
            Contract.ThrowIfNull(nextCommandTarget);

            CurrentHandlers = _commandHandlerServiceFactory.GetService(WpfTextView);
            NextCommandTarget = nextCommandTarget;

            return this;
        }

        protected virtual ITextBuffer GetSubjectBufferContainingCaret()
        {
            return WpfTextView.GetBufferContainingCaret();
        }

        protected virtual ITextView ConvertTextView()
        {
            return WpfTextView;
        }
    }
}
