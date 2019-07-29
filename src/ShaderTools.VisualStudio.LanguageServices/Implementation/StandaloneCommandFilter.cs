// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;

namespace ShaderTools.VisualStudio.LanguageServices.Implementation
{
    /// <summary>
    /// A CommandFilter used for "normal" files, as opposed to Venus files which are special.
    /// </summary>
    internal sealed class StandaloneCommandFilter : AbstractOleCommandTarget
    {
        /// <summary>
        /// Creates a new command handler that is attached to an IVsTextView.
        /// </summary>
        /// <param name="wpfTextView">The IWpfTextView of the view.</param>
        /// <param name="commandHandlerServiceFactory">The MEF imported ICommandHandlerServiceFactory.</param>
        /// <param name="editorAdaptersFactoryService">The editor adapter</param>
        internal StandaloneCommandFilter(
            IWpfTextView wpfTextView,
            IEditorCommandHandlerServiceFactory commandHandlerServiceFactory,
            IVsEditorAdaptersFactoryService editorAdaptersFactoryService)
            : base(wpfTextView, commandHandlerServiceFactory, editorAdaptersFactoryService)
        {
            wpfTextView.Closed += OnTextViewClosed;
            wpfTextView.BufferGraph.GraphBufferContentTypeChanged += OnGraphBuffersChanged;
            wpfTextView.BufferGraph.GraphBuffersChanged += OnGraphBuffersChanged;

            RefreshCommandFilters();
        }

        private void OnGraphBuffersChanged(object sender, EventArgs e)
        {
            RefreshCommandFilters();
        }

        private void OnTextViewClosed(object sender, EventArgs e)
        {
            WpfTextView.Closed -= OnTextViewClosed;
            WpfTextView.BufferGraph.GraphBufferContentTypeChanged -= OnGraphBuffersChanged;
            WpfTextView.BufferGraph.GraphBuffersChanged -= OnGraphBuffersChanged;
        }
    }
}
