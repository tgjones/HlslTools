// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Formatting;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.Utilities.Diagnostics;
using ShaderTools.VisualStudio.LanguageServices.Implementation;

namespace ShaderTools.VisualStudio.LanguageServices.LanguageService
{
    internal abstract partial class AbstractLanguageService<TPackage, TLanguageService> : AbstractLanguageService
        where TPackage : AbstractPackage<TPackage, TLanguageService>
        where TLanguageService : AbstractLanguageService<TPackage, TLanguageService>
    {
        internal TPackage Package { get; }

        // Note: The lifetime for state in this class is carefully managed.  For every bit of state
        // we set up, there is a corresponding tear down phase which deconstructs the state in the
        // reverse order it was created in.
        internal VisualStudioWorkspace Workspace { get; private set; }
        internal IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; private set; }

        /// <summary>
        /// Whether or not we have been set up. This is set once everything is wired up and cleared once tear down has begun.
        /// </summary>
        /// <remarks>
        /// We don't set this until we've completed setup. If something goes sideways during it, we will never register
        /// with the shell and thus have a floating thing around that can't be safely shut down either. We're in a bad
        /// state but trying to proceed will only make things worse.
        /// </remarks>
        private bool _isSetUp;

        protected AbstractLanguageService(
            TPackage package)
        {
            this.Package = package;
        }

        public override IServiceProvider SystemServiceProvider
        {
            get
            {
                return this.Package;
            }
        }

        /// <summary>
        /// Setup and TearDown go in reverse order.
        /// </summary>
        internal void Setup()
        {
            // First, acquire any services we need throughout our lifetime.
            this.GetServices();

            var componentModel = this.Package.ComponentModel;

            // Start off a background task to prime some components we'll need for editing
            VsTaskLibraryHelper.CreateAndStartTask(VsTaskLibraryHelper.ServiceInstance, VsTaskRunContext.BackgroundThread,
                () => PrimeLanguageServiceComponentsOnBackground(componentModel));

            // Next, make any connections to these services.
            this.ConnectToServices();

            // Finally, once our connections are established, set up any initial state that we need.
            // Note: we may be instantiated at any time (including when the IDE is already
            // debugging).  We must not assume anything about our initial state and must instead
            // query for all the information we need at this point.
            this.Initialize();

            _isSetUp = true;
        }

        internal void TearDown()
        {
            if (!_isSetUp)
            {
                throw new InvalidOperationException();
            }

            _isSetUp = false;
            GC.SuppressFinalize(this);

            this.Uninitialize();
            this.DisconnectFromServices();
            this.RemoveServices();
        }

        ~AbstractLanguageService()
        {
            if (!Environment.HasShutdownStarted && _isSetUp)
            {
                throw new InvalidOperationException("TearDown not called!");
            }
        }

        protected virtual void GetServices()
        {
            // This method should only contain calls to acquire services off of the component model
            // or service providers.  Anything else which is more complicated should go in Initialize
            // instead.
            this.Workspace = this.Package.Workspace;
            this.EditorAdaptersFactoryService = this.Package.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
        }

        protected virtual void RemoveServices()
        {
            this.EditorAdaptersFactoryService = null;
            this.Workspace = null;
        }

        /// <summary>
        /// Keep ConnectToServices and DisconnectFromServices in 1:1 correspondence.
        /// DisconnectFromServices should clean up resources in the reverse direction that they are
        /// initialized in.
        /// </summary>
        protected virtual void ConnectToServices()
        {
            
        }

        protected virtual void DisconnectFromServices()
        {
            
        }

        /// <summary>
        /// Called right after we instantiate the language service.  Used to set up any internal
        /// state we need.
        /// 
        /// Try to keep this method fairly clean.  Any complicated logic should go in methods called
        /// from this one.  Initialize and Uninitialize go in reverse order 
        /// </summary>
        protected virtual void Initialize()
        {
            
        }

        protected virtual void Uninitialize()
        {
            
        }

        private void PrimeLanguageServiceComponentsOnBackground(IComponentModel componentModel)
        {
            var formatter = this.Workspace.Services.GetLanguageServices(ShaderToolsLanguageName).GetService<ISyntaxFormattingService>();
            if (formatter != null)
            {
                // TODO
                //formatter.GetDefaultFormattingRules();
            }
        }

        protected abstract string ContentTypeName { get; }
        protected abstract string LanguageName { get; }
        protected abstract string ShaderToolsLanguageName { get; }

        protected virtual void SetupNewTextView(IVsTextView textView)
        {
            Contract.ThrowIfNull(textView);

            var wpfTextView = EditorAdaptersFactoryService.GetWpfTextView(textView);
            Contract.ThrowIfNull(wpfTextView, "Could not get IWpfTextView for IVsTextView");

            Contract.Assert(!wpfTextView.Properties.ContainsProperty(typeof(AbstractOleCommandTarget)));

            var commandHandlerFactory = Package.ComponentModel.GetService<IEditorCommandHandlerServiceFactory>();
            var workspace = Package.ComponentModel.GetService<VisualStudioWorkspace>();

            // The lifetime of CommandFilter is married to the view
            wpfTextView.GetOrCreateAutoClosingProperty(v =>
                new StandaloneCommandFilter(v, commandHandlerFactory, EditorAdaptersFactoryService).AttachToVsTextView());

            ConditionallyCollapseOutliningRegions(wpfTextView, workspace);
        }

        private void ConditionallyCollapseOutliningRegions(IWpfTextView wpfTextView, Workspace workspace)
        {
            var outliningManagerService = Package.ComponentModel.GetService<IOutliningManagerService>();
            var outliningManager = outliningManagerService.GetOutliningManager(wpfTextView);
            if (outliningManager == null)
            {
                return;
            }

            if (!workspace.Options.GetOption(FeatureOnOffOptions.Outlining, this.ShaderToolsLanguageName))
            {
                outliningManager.Enabled = false;
            }
        }
    }
}