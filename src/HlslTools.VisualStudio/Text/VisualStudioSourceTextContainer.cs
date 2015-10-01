using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HlslTools.VisualStudio.Text
{
    internal sealed class VisualStudioSourceTextContainer
    {
        public VisualStudioSourceTextContainer(ITextBuffer textBuffer)
        {
            IVsTextBuffer vsTextBuffer;
            if (textBuffer.Properties.TryGetProperty(typeof(IVsTextBuffer), out vsTextBuffer))
            {
                var persistFileFormat = vsTextBuffer as IPersistFileFormat;

                string ppzsFilename = null;
                uint iii;
                persistFileFormat?.GetCurFile(out ppzsFilename, out iii);

                Filename = ppzsFilename;
            }
        }

        public string Filename { get; }
    }
}