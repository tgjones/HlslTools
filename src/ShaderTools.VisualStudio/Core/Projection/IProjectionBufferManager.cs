using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace ShaderTools.VisualStudio.Core.Projection
{
    //  Graph:
    //      View Buffer [ContentType = ShaderLab Projection]
    //        |      \
    //        |    Secondary [ContentType = HLSL]
    //        |      /
    //       Disk Buffer [ContentType = ShaderLab]
    internal interface IProjectionBufferManager : IDisposable
    {
        /// <summary>
        /// Projection buffer that is presented in the view.
        /// Content type typically derives from 'projection'.
        /// </summary>
        IProjectionBuffer ViewBuffer { get; }

        /// <summary>
        /// Buffer that contains original document that was loaded from disk.
        /// </summary>
        ITextBuffer DiskBuffer { get; }

        event EventHandler MappingsChanged;
    }
}