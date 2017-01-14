using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.Unity.Syntax;

namespace ShaderTools.Editor.VisualStudio.Core.Projection
{
    /// <summary>
    /// Manages the projection buffer for the primary language
    /// </summary>
    internal sealed class ProjectionBufferManager : IProjectionBufferManager
    {
        private const string _inertContentTypeName = "inert";

        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private int? _savedCaretPosition;

        public ProjectionBufferManager(ITextBuffer diskBuffer,
                                       IProjectionBufferFactoryService projectionBufferFactoryService,
                                       IContentTypeRegistryService contentTypeRegistryService,
                                       string secondaryContentTypeName)
        {
            DiskBuffer = diskBuffer;

            _contentTypeRegistryService = contentTypeRegistryService;

            var snapshot = diskBuffer.CurrentSnapshot;

            var shaderLabSyntaxTree = SyntaxFactory.ParseUnitySyntaxTree(new Text.VisualStudioSourceText(snapshot, null, true));

            var cgBlockVisitor = new CgBlockVisitor();
            //cgBlockVisitor.Visit(shaderLabSyntaxTree.Root);
            var cgBlockSpans = cgBlockVisitor.CgBlockSpans;

            var dataBufferSpans = new List<object>();

            var secondaryContentType = _contentTypeRegistryService.GetContentType(secondaryContentTypeName);
            var primaryIndex = 0;
            foreach (var cgBlockSpan in cgBlockSpans)
            {
                var primarySpan = Span.FromBounds(primaryIndex, cgBlockSpan.Start);
                if (!primarySpan.IsEmpty)
                    dataBufferSpans.Add(snapshot.CreateTrackingSpan(primarySpan, SpanTrackingMode.EdgeExclusive));

                var elisionBuffer = projectionBufferFactoryService.CreateElisionBuffer(null, 
                    new NormalizedSnapshotSpanCollection(new SnapshotSpan(snapshot, cgBlockSpan)),
                    ElisionBufferOptions.None, secondaryContentType);

                dataBufferSpans.Add(elisionBuffer.CurrentSnapshot.CreateTrackingSpan(0, elisionBuffer.CurrentSnapshot.Length, SpanTrackingMode.EdgeInclusive));

                primaryIndex = cgBlockSpan.End;
            }

            // Last span.
            {
                var primarySpan = Span.FromBounds(primaryIndex, snapshot.Length);
                if (!primarySpan.IsEmpty)
                    dataBufferSpans.Add(snapshot.CreateTrackingSpan(primarySpan, SpanTrackingMode.EdgeExclusive));
            }

            ViewBuffer = projectionBufferFactoryService.CreateProjectionBuffer(null, dataBufferSpans, ProjectionBufferOptions.None);

            DiskBuffer.Properties.AddProperty(typeof(IProjectionBufferManager), this);
            ViewBuffer.Properties.AddProperty(typeof(IProjectionBufferManager), this);
        }

        private sealed class CgBlockVisitor : SyntaxWalker
        {
            public List<Span> CgBlockSpans { get; } = new List<Span>();

            public override void VisitCgProgram(CgProgramSyntax node)
            {
                throw new NotImplementedException();
                //CgBlockSpans.Add(new Span(node.CgProgramKeyword.Span.End, node.EndCgKeyword.Span.Start - node.CgProgramKeyword.Span.End));
            }

            public override void VisitCgInclude(CgIncludeSyntax node)
            {
                throw new NotImplementedException();
                //CgBlockSpans.Add(new Span(node.CgIncludeKeyword.Span.End, node.EndCgKeyword.Span.Start - node.CgIncludeKeyword.Span.End));
            }
        }

        #region IProjectionBufferManager

        //  Graph:
        //      View Buffer [ContentType = ShaderLab Projection]
        //        |      \
        //        |    Secondary [ContentType = HLSL]
        //        |      /
        //       Disk Buffer [ContentType = ShaderLab]
        public IProjectionBuffer ViewBuffer { get; }
        public ITextBuffer DiskBuffer { get; }

        public event EventHandler MappingsChanged;

        public void Dispose()
        {
            DiskBuffer.Properties.RemoveProperty(typeof(IProjectionBufferManager));
            ViewBuffer.Properties.RemoveProperty(typeof(IProjectionBufferManager));
        }
        #endregion
    }
}