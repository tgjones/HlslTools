// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Shared.TestHooks;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.NavigationBar
{
    [Export(typeof(INavigationBarControllerFactoryService))]
    internal class NavigationBarControllerFactoryService : INavigationBarControllerFactoryService
    {
        private readonly IUIThreadOperationExecutor _uIThreadOperationExecutor;
        private readonly AggregateAsynchronousOperationListener _asyncListener;

        [ImportingConstructor]
        public NavigationBarControllerFactoryService(
            IUIThreadOperationExecutor uIThreadOperationExecutor,
            [ImportMany] IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> asyncListeners)
        {
            _uIThreadOperationExecutor = uIThreadOperationExecutor;
            _asyncListener = new AggregateAsynchronousOperationListener(asyncListeners, FeatureAttribute.NavigationBar);
        }

        public INavigationBarController CreateController(INavigationBarPresenter presenter, ITextBuffer textBuffer)
        {
            return new NavigationBarController(
                presenter,
                textBuffer,
                _uIThreadOperationExecutor,
                _asyncListener);
        }
    }
}
