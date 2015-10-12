using System.IO;
using HlslTools.Text;

namespace HlslTools.VisualStudio.Text
{
    internal sealed class VisualStudioFileSystem : IIncludeFileSystem
    {
        private readonly VisualStudioSourceTextFactory _sourceTextFactory;
        private readonly string _parentDirectory;

        public VisualStudioFileSystem(VisualStudioSourceTextContainer textContainer, VisualStudioSourceTextFactory sourceTextFactory)
        {
            _sourceTextFactory = sourceTextFactory;
            if (textContainer.Filename != null)
                _parentDirectory = Path.GetDirectoryName(textContainer.Filename);
        }

        public SourceText GetInclude(string path)
        {
            // TODO: Look in running document table for requested file.

            if (_parentDirectory == null)
                return null;

            var fullPath = Path.Combine(_parentDirectory, path);
            if (!File.Exists(fullPath))
                return null;

            return _sourceTextFactory.CreateSourceText(fullPath);
        }

//        if (textBuffer.Properties.ContainsProperty(typeof(IVsTextBuffer)))
//            {
//                IObjectWithSite objectWithSite = textBuffer.Properties.GetProperty<IObjectWithSite>(typeof(IVsTextBuffer));
//                if (objectWithSite != null)
//                {
//                    Guid serviceProviderGuid = typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider).GUID;
//        IntPtr ppServiceProvider = IntPtr.Zero;
//        // Get the service provider pointer using the Guid of the OleInterop ServiceProvider
//        objectWithSite.GetSite(ref serviceProviderGuid, out ppServiceProvider);
 
//                    if (ppServiceProvider != IntPtr.Zero)
//                    {
//                        // Create a System.ServiceProvider with the OleInterop ServiceProvider
//                        OleInterop.IServiceProvider oleInteropServiceProvider = (OleInterop.IServiceProvider)Marshal.GetObjectForIUnknown(ppServiceProvider);
//                        return new Microsoft.VisualStudio.Shell.ServiceProvider(oleInteropServiceProvider);
//                    }
//}
//            }
    }
}