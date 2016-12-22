using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.VisualStudio.Language.Intellisense;

namespace ShaderTools.VisualStudio.Core.Glyphs
{
    // Based on https://github.com/tunnelvisionlabs/LangSvcV2/blob/master/Tvl.VisualStudio.Language.Implementation/Intellisense/DispatcherGlyphService.cs
    [Export]
    internal sealed class DispatcherGlyphService
    {
        private readonly ConcurrentDictionary<uint, ImageSource> _glyphCache = new ConcurrentDictionary<uint, ImageSource>();
        private readonly ConcurrentDictionary<uint, Icon> _iconCache = new ConcurrentDictionary<uint, Icon>();

        public DispatcherGlyphService()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        [Import]
        public IGlyphService GlyphService { get; private set; }

        public Dispatcher Dispatcher { get; }

        public ImageSource GetGlyph(StandardGlyphGroup group, StandardGlyphItem item)
        {
            var key = ((uint)group << 16) + (uint)item;
            return _glyphCache.GetOrAdd(key, CreateGlyph);
        }

        private ImageSource CreateGlyph(uint key)
        {
            var group = (StandardGlyphGroup) (key >> 16);
            var item = (StandardGlyphItem) (key & 0xFFFF);
            ImageSource source = null;

            // create the glyph on the UI thread
            var dispatcher = Dispatcher;
            dispatcher?.Invoke(() => source = GlyphService.GetGlyph(group, item));
            return source;
        }

        public Icon GetIcon(StandardGlyphGroup group, StandardGlyphItem item)
        {
            var key = ((uint) group << 16) + (uint) item;
            return _iconCache.GetOrAdd(key, CreateIcon);
        }

        private Icon CreateIcon(uint key)
        {
            var imageSource = (BitmapSource) CreateGlyph(key);

            var bmpEncoder = new PngBitmapEncoder();
            bmpEncoder.Frames.Add(BitmapFrame.Create(imageSource));

            var s = new MemoryStream();
            bmpEncoder.Save(s);

            s.Position = 0;

            var bitmap = new Bitmap(s);
            var icon = Icon.FromHandle(bitmap.GetHicon());

            return icon;
        }
    }
}