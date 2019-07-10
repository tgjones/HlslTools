using System;
using System.Collections.Generic;
using System.Composition;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Shared.TestHooks;
using IWorkspaceService = Microsoft.CodeAnalysis.Host.IWorkspaceService;

namespace ShaderTools.CodeAnalysis.Editor.ShaderLab.Projection
{
    [ExportWorkspaceServiceFactory(typeof(IProjectionBufferService))]
    internal sealed class ProjectionBufferServiceFactory : IWorkspaceServiceFactory
    {
        private readonly IProjectionBufferFactoryService _projectionBufferFactoryService;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private readonly IAsynchronousOperationListener _asyncListener;
        private readonly IForegroundNotificationService _notificationService;

        [ImportingConstructor]
        public ProjectionBufferServiceFactory(
            IProjectionBufferFactoryService projectionBufferFactoryService,
            IContentTypeRegistryService contentTypeRegistryService,
            [ImportMany] IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> asyncListeners,
            IForegroundNotificationService notificationService)
        {
            _projectionBufferFactoryService = projectionBufferFactoryService;
            _contentTypeRegistryService = contentTypeRegistryService;

            _asyncListener = new AggregateAsynchronousOperationListener(asyncListeners, FeatureAttribute.GraphProvider);
            _notificationService = notificationService;
        }

        public IWorkspaceService CreateService(HostWorkspaceServices workspaceServices)
        {
            return new ProjectionBufferService(
                _projectionBufferFactoryService,
                _contentTypeRegistryService,
                _notificationService,
                _asyncListener,
                workspaceServices.Workspace);
        }
    }
}
