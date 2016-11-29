using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace ShaderTools.VisualStudio.Hlsl.Navigation
{
    // Based on https://github.com/tunnelvisionlabs/LangSvcV2/blob/master/Tvl.VisualStudio.Text.Implementation/Navigation/EditorNavigationDropdownBar.cs
    [ComVisible(true)]
    internal sealed class EditorNavigationDropdownBarClient : IVsDropdownBarClient, IVsCodeWindowEvents, IVsTextViewEvents
    {
        private readonly IVsCodeWindow _codeWindow;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactory;
        private readonly EditorNavigationSource _source;
        private readonly IBufferGraphFactoryService _bufferGraphFactoryService;
        private readonly Dispatcher _dispatcher;
        private readonly ImageList _imageList;
        private readonly Dictionary<ImageSource, int> _glyphIndexes = new Dictionary<ImageSource, int>();

        private uint _codeWindowEventsCookie;
        private readonly Dictionary<IVsTextView, uint> _textViewEventsCookies = new Dictionary<IVsTextView, uint>();

        private IWpfTextView _currentTextView;
        private IVsDropdownBar _dropdownBar;

        private List<EditorTypeNavigationTarget> _navigationItems;

        private int _selectedType;

        public EditorNavigationDropdownBarClient(IVsCodeWindow codeWindow, IVsEditorAdaptersFactoryService editorAdaptersFactory, EditorNavigationSource source, IBufferGraphFactoryService bufferGraphFactoryService)
        {
            _codeWindow = codeWindow;
            _editorAdaptersFactory = editorAdaptersFactory;
            _source = source;
            _bufferGraphFactoryService = bufferGraphFactoryService;
            _currentTextView = editorAdaptersFactory.GetWpfTextView(codeWindow.GetLastActiveView());
            _dispatcher = _currentTextView.VisualElement.Dispatcher;
            _imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit
            };

            var connectionPointContainer = codeWindow as IConnectionPointContainer;
            if (connectionPointContainer != null)
            {
                var textViewEventsGuid = typeof(IVsCodeWindowEvents).GUID;
                IConnectionPoint connectionPoint;
                connectionPointContainer.FindConnectionPoint(ref textViewEventsGuid, out connectionPoint);
                connectionPoint?.Advise(this, out _codeWindowEventsCookie);
            }

            var primaryView = codeWindow.GetPrimaryView();
            if (primaryView != null)
                ((IVsCodeWindowEvents)this).OnNewView(primaryView);

            var secondaryView = codeWindow.GetSecondaryView();
            if (secondaryView != null)
                ((IVsCodeWindowEvents)this).OnNewView(secondaryView);

            _navigationItems = new List<EditorTypeNavigationTarget>();

            source.NavigationTargetsChanged += OnNavigationTargetsChanged;
            UpdateNavigationTargets();

            _currentTextView.Caret.PositionChanged += OnCaretPositionChanged;
        }

        public int DropdownCount => 2;
        public bool Updating { get; private set; }
        public bool Enabled => false;

        private EditorNavigationTarget GetSelectedItem(int combo, int index)
        {
            return (combo == 0) ? _navigationItems[index] : _navigationItems[_selectedType].Children[index];
        }

        #region IVsDropdownBarClient Members

        public int GetComboAttributes(int iCombo, out uint pcEntries, out uint puEntryType, out IntPtr phImageList)
        {
            pcEntries = 0;
            puEntryType = 0;
            phImageList = IntPtr.Zero;
            if (!ValidateCombo(iCombo))
                return VSConstants.E_INVALIDARG;

            switch (iCombo)
            {
                case 0:
                    pcEntries = (uint) _navigationItems.Count;
                    break;
                case 1:
                    pcEntries = (_navigationItems.Any()) ? (uint)_navigationItems[_selectedType].Children.Count : 0;
                    break;
            }
            
            puEntryType = (uint)(DROPDOWNENTRYTYPE.ENTRY_IMAGE | DROPDOWNENTRYTYPE.ENTRY_TEXT | DROPDOWNENTRYTYPE.ENTRY_ATTR);
            phImageList = _imageList.Handle;
            return VSConstants.S_OK;
        }

        public int GetComboTipText(int iCombo, out string pbstrText)
        {
            pbstrText = null;
            return VSConstants.S_OK;
        }

        public int GetEntryAttributes(int iCombo, int iIndex, out uint pAttr)
        {
            pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_PLAIN;
            if (GetSelectedItem(iCombo, iIndex).IsGray)
                pAttr |= (uint)DROPDOWNFONTATTR.FONTATTR_GRAY;

            return VSConstants.S_OK;
        }

        public int GetEntryImage(int iCombo, int iIndex, out int piImageIndex)
        {
            piImageIndex = 0;

            var targetGlyph = GetSelectedItem(iCombo, iIndex).Glyph;
            if (targetGlyph == null)
                return VSConstants.S_FALSE;

            int index;
            if (!_glyphIndexes.TryGetValue(targetGlyph, out index))
            {
                index = -1;

                // add the image to the image list
                BitmapSource bitmapSource = targetGlyph as BitmapSource;
                if (bitmapSource != null)
                {
                    if (bitmapSource.Format != PixelFormats.Pbgra32 && bitmapSource.Format != PixelFormats.Bgra32)
                    {
                        var formattedBitmapSource = new FormatConvertedBitmap();
                        formattedBitmapSource.BeginInit();
                        formattedBitmapSource.Source = bitmapSource;
                        formattedBitmapSource.DestinationFormat = PixelFormats.Pbgra32;
                        formattedBitmapSource.EndInit();

                        bitmapSource = formattedBitmapSource;
                    }

                    int bytesPerPixel = bitmapSource.Format.BitsPerPixel / 8;
                    byte[] data = new byte[bitmapSource.PixelWidth * bitmapSource.PixelHeight * bytesPerPixel];
                    int stride = bitmapSource.PixelWidth * bytesPerPixel;
                    bitmapSource.CopyPixels(data, stride, 0);
                    IntPtr nativeData = Marshal.AllocHGlobal(data.Length);
                    try
                    {
                        Marshal.Copy(data, 0, nativeData, data.Length);
                        PixelFormat pixelFormat = (bitmapSource.Format == PixelFormats.Bgra32) ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppPArgb;
                        Bitmap bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, stride, pixelFormat, nativeData);
                        _imageList.Images.Add(bitmap);
                        index = _imageList.Images.Count - 1;
                        _glyphIndexes.Add(targetGlyph, index);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(nativeData);
                    }
                }
            }

            if (index >= 0)
                piImageIndex = index;

            return index >= 0 ? VSConstants.S_OK : VSConstants.S_FALSE;
        }

        public int GetEntryText(int iCombo, int iIndex, out string ppszText)
        {
            ppszText = GetSelectedItem(iCombo, iIndex).Name;
            return ppszText != null ? VSConstants.S_OK : VSConstants.E_FAIL;
        }

        public int OnComboGetFocus(int iCombo)
        {
            return VSConstants.S_OK;
        }

        public int OnItemChosen(int iCombo, int iIndex)
        {
            if (Updating)
                return VSConstants.E_FAIL;

            try
            {
                var target = GetSelectedItem(iCombo, iIndex);
                if (target != null)
                {
                    _currentTextView.NavigateTo(_bufferGraphFactoryService, target.Span, target.Seek);
                }
            }
            catch (Exception ex)
            {
                if (ex.IsCritical())
                    throw;

                return Marshal.GetHRForException(ex);
            }

            return VSConstants.S_OK;
        }

        public int OnItemSelected(int iCombo, int iIndex)
        {
            if (!ValidateIndex(iCombo, iIndex))
                return VSConstants.E_INVALIDARG;

            return VSConstants.S_OK;
        }

        public int SetDropdownBar(IVsDropdownBar pDropdownBar)
        {
            _dropdownBar = pDropdownBar;
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsCodeWindowEvents Members

        int IVsCodeWindowEvents.OnCloseView(IVsTextView pView)
        {
            if (pView == null)
                throw new ArgumentNullException(nameof(pView));

            uint cookie;
            if (_textViewEventsCookies.TryGetValue(pView, out cookie))
            {
                _textViewEventsCookies.Remove(pView);

                IConnectionPointContainer connectionPointContainer = pView as IConnectionPointContainer;
                if (connectionPointContainer != null)
                {
                    Guid textViewEventsGuid = typeof(IVsTextViewEvents).GUID;
                    IConnectionPoint connectionPoint;
                    connectionPointContainer.FindConnectionPoint(ref textViewEventsGuid, out connectionPoint);
                    connectionPoint?.Unadvise(cookie);
                }
            }

            return VSConstants.S_OK;
        }

        int IVsCodeWindowEvents.OnNewView(IVsTextView pView)
        {
            if (pView == null)
                throw new ArgumentNullException(nameof(pView));

            IConnectionPointContainer connectionPointContainer = pView as IConnectionPointContainer;
            if (connectionPointContainer != null)
            {
                Guid textViewEventsGuid = typeof(IVsTextViewEvents).GUID;
                IConnectionPoint connectionPoint;
                connectionPointContainer.FindConnectionPoint(ref textViewEventsGuid, out connectionPoint);
                if (connectionPoint != null)
                {
                    uint cookie;
                    connectionPoint.Advise(this, out cookie);
                    if (cookie != 0)
                        _textViewEventsCookies.Add(pView, cookie);
                }
            }

            return VSConstants.S_OK;
        }

        #endregion

        #region IVsTextViewEvents Members

        void IVsTextViewEvents.OnChangeCaretLine(IVsTextView pView, int iNewLine, int iOldLine)
        {
        }

        void IVsTextViewEvents.OnChangeScrollInfo(IVsTextView pView, int iBar, int iMinUnit, int iMaxUnits, int iVisibleUnits, int iFirstVisibleUnit)
        {
        }

        void IVsTextViewEvents.OnKillFocus(IVsTextView pView)
        {
        }

        void IVsTextViewEvents.OnSetBuffer(IVsTextView pView, IVsTextLines pBuffer)
        {
        }

        void IVsTextViewEvents.OnSetFocus(IVsTextView pView)
        {
            IWpfTextView textView = pView != null ? _editorAdaptersFactory.GetWpfTextView(pView) : null;
            if (textView == _currentTextView)
            {
                IVsTextView activeView = _codeWindow.GetLastActiveView();
                if (activeView != pView)
                    return;

                return;
            }

            if (_currentTextView != null)
                _currentTextView.Caret.PositionChanged -= OnCaretPositionChanged;

            _currentTextView = textView;

            if (_currentTextView != null)
            {
                _currentTextView.Caret.PositionChanged += OnCaretPositionChanged;
                UpdateNavigationTargets();
            }
        }

        #endregion

        private static bool ValidateCombo(int combo)
        {
            return combo >= 0 && combo < 2;
        }

        private bool ValidateIndex(int combo, int index)
        {
            return ValidateCombo(combo) && index >= 0 && index < _navigationItems[_selectedType].Children.Count;
        }

        private void UpdateNavigationTargets()
        {
            lock (this)
            {
                if (Updating)
                    return;
            }

            try
            {
                Updating = true;
                var targets = _source.GetNavigationTargets().ToList();
                Action action = () => UpdateNavigationTargets(targets);
                if (_dispatcher.Thread == Thread.CurrentThread)
                    action();
                else
                    _dispatcher.Invoke(action);
            }
            finally
            {
                Updating = false;
            }
        }

        private void UpdateNavigationTargets(List<EditorTypeNavigationTarget> targets)
        {
            _navigationItems = targets;
            UpdateSelectedNavigationTargets();
        }

        private void OnNavigationTargetsChanged(object sender, EventArgs e)
        {
            UpdateNavigationTargets();
        }

        private void UpdateSelectedNavigationTargets()
        {
            if (Thread.CurrentThread != _dispatcher.Thread)
            {
                _dispatcher.Invoke(UpdateSelectedNavigationTargets);
                return;
            }

            try
            {
                UpdateSelectedNavigationTargetsImpl();
            }
            catch (Exception ex)
            {
                if (ex.IsCritical())
                    throw;
            }
        }

        private void UpdateSelectedNavigationTargetsImpl()
        {
            if (!_navigationItems.Any())
                return;

            var currentPosition = _currentTextView.Caret.Position.BufferPosition;

            // Set selected type.
            _selectedType = 0;
            var typeIndex = 0;
            foreach (var navigationItem in _navigationItems)
            {
                if (navigationItem.Span.Snapshot == currentPosition.Snapshot && navigationItem.Span.Contains(currentPosition))
                    _selectedType = typeIndex;
                ++typeIndex;
            }

            var navigationMembers = _navigationItems[_selectedType].Children;

            foreach (var navigationMember in navigationMembers)
                navigationMember.IsGray = false;

            // Set selected member - if caret is not on a member, then fallback
            // to the next member, but grey it out.
            var selectedMember = -1;
            for (var i = 0; i < navigationMembers.Count; i++)
            {
                var navigationItem = navigationMembers[i];
                if (navigationItem.Span.Snapshot != currentPosition.Snapshot)
                    continue;
                if (currentPosition < navigationMembers[i].Span.Start)
                {
                    selectedMember = i;
                    navigationMembers[i].IsGray = true;
                    break;
                }
                if (navigationItem.Span.Contains(currentPosition))
                {
                    selectedMember = i;
                    break;
                }
            }
            if (selectedMember == -1 && navigationMembers.Count > 0)
            {
                selectedMember = navigationMembers.Count - 1;
                navigationMembers[selectedMember].IsGray = true;
            }
            if (selectedMember == -1)
                selectedMember = 0;

            bool wasUpdating = Updating;
            try
            {
                Updating = true;
                if (_dropdownBar != null)
                {
                    _dropdownBar.RefreshCombo(0, _selectedType);
                    _dropdownBar.RefreshCombo(1, selectedMember);
                }
            }
            finally
            {
                Updating = wasUpdating;
            }
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateSelectedNavigationTargets();
        }
    }
}