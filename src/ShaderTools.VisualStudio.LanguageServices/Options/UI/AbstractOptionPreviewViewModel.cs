// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Editor;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Editor.Shared.Preview;
using ShaderTools.CodeAnalysis.Formatting;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Threading;
using ShaderTools.VisualStudio.LanguageServices.Utilities;
using OptionSet = ShaderTools.CodeAnalysis.Options.OptionSet;

namespace ShaderTools.VisualStudio.LanguageServices.Options.UI
{
    internal abstract class AbstractOptionPreviewViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        private IComponentModel _componentModel;
        private IWpfTextViewHost _textViewHost;

        private IContentType _contentType;
        private IEditorOptionsFactoryService _editorOptions;
        private ITextEditorFactoryService _textEditorFactoryService;
        private ITextBufferFactoryService _textBufferFactoryService;
        private IProjectionBufferFactoryService _projectionBufferFactory;
        private IContentTypeRegistryService _contentTypeRegistryService;

        public List<object> Items { get; set; }

        public OptionSet Options { get; set; }
        private readonly OptionSet _originalOptions;

        protected AbstractOptionPreviewViewModel(OptionSet options, IServiceProvider serviceProvider, string language)
        {
            this.Options = options;
            _originalOptions = options;
            this.Items = new List<object>();

            _componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));

            _contentTypeRegistryService = _componentModel.GetService<IContentTypeRegistryService>();
            _textBufferFactoryService = _componentModel.GetService<ITextBufferFactoryService>();
            _textEditorFactoryService = _componentModel.GetService<ITextEditorFactoryService>();
            _projectionBufferFactory = _componentModel.GetService<IProjectionBufferFactoryService>();
            _editorOptions = _componentModel.GetService<IEditorOptionsFactoryService>();
            this.Language = language;

            _contentType = _contentTypeRegistryService.GetContentType(ContentTypeNames.HlslContentType);
        }

        internal OptionSet ApplyChangedOptions(OptionSet optionSet)
        {
            foreach (var optionKey in this.Options.GetChangedOptions(_originalOptions))
            {
                optionSet = optionSet.WithChangedOption(optionKey, this.Options.GetOption(optionKey));
            }

            return optionSet;
        }

        public void SetOptionAndUpdatePreview<T>(T value, IOption option, string preview)
        {
            if (option is Option<T>)
            {
                Options = Options.WithChangedOption((Option<T>)option, value);
            }
            else if (option is PerLanguageOption<T>)
            {
                Options = Options.WithChangedOption((PerLanguageOption<T>)option, Language, value);
            }
            else
            {
                throw new InvalidOperationException("Unexpected option type");
            }

            UpdateDocument(preview);
        }

        public IWpfTextViewHost TextViewHost
        {
            get
            {
                return _textViewHost;
            }

            private set
            {
                // make sure we close previous view.
                if (_textViewHost != null)
                {
                    _textViewHost.Close();
                }

                SetProperty(ref _textViewHost, value);
            }
        }

        public string Language { get; }

        public void UpdatePreview(string text)
        {
            const string start = "//[";
            const string end = "//]";

            var service = MefV1HostServices.Create(_componentModel.DefaultExportProvider);
            var workspace = new PreviewWorkspace(service);

            var document = workspace.OpenDocument(DocumentId.CreateNewId("document"), SourceText.From(text), Language);
            var formatted = Formatter.FormatAsync(document, this.Options).WaitAndGetResult(CancellationToken.None);

            var textBuffer = _textBufferFactoryService.CreateTextBuffer(formatted.SourceText.ToString(), _contentType);

            var container = textBuffer.AsTextContainer();
            var documentBackedByTextBuffer = document.WithText(container.CurrentText);

            var bufferText = textBuffer.CurrentSnapshot.GetText().ToString();
            var startIndex = bufferText.IndexOf(start, StringComparison.Ordinal);
            var endIndex = bufferText.IndexOf(end, StringComparison.Ordinal);
            var startLine = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(startIndex) + 1;
            var endLine = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(endIndex);

            var projection = _projectionBufferFactory.CreateProjectionBufferWithoutIndentation(_contentTypeRegistryService,
                _editorOptions.CreateOptions(),
                textBuffer.CurrentSnapshot,
                "",
                LineSpan.FromBounds(startLine, endLine));

            var textView = _textEditorFactoryService.CreateTextView(projection,
              _textEditorFactoryService.CreateTextViewRoleSet());

            this.TextViewHost = _textEditorFactoryService.CreateTextViewHost(textView, setFocus: false);

            workspace.CloseDocument(document.Id);
            workspace.OpenDocument(document.Id, documentBackedByTextBuffer.SourceText, Language);
            //workspace.UpdateDocument(documentBackedByTextBuffer.Id, documentBackedByTextBuffer.SourceText);
        }

        public void Dispose()
        {
            if (_textViewHost != null)
            {
                _textViewHost.Close();
                _textViewHost = null;
            }
        }

        private void UpdateDocument(string text)
        {
            UpdatePreview(text);
        }
    }
}
