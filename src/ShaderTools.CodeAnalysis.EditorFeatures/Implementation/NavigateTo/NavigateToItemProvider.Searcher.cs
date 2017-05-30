// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
using ShaderTools.CodeAnalysis.NavigateTo;
using ShaderTools.CodeAnalysis.Shared.TestHooks;
using ShaderTools.CodeAnalysis.Shared.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.NavigateTo
{
    internal partial class NavigateToItemProvider
    {
        private class Searcher
        {
            private readonly Workspace _workspace;
            private readonly INavigateToItemDisplayFactory _displayFactory;
            private readonly INavigateToCallback _callback;
            private readonly string _searchPattern;
            private readonly bool _searchCurrentDocument;
            private readonly ImmutableArray<Document> _currentDocuments;
            private readonly ProgressTracker _progress;
            private readonly IAsynchronousOperationListener _asyncListener;
            private readonly CancellationToken _cancellationToken;

            public Searcher(
                Workspace workspace,
                IAsynchronousOperationListener asyncListener,
                INavigateToItemDisplayFactory displayFactory,
                INavigateToCallback callback,
                string searchPattern,
                bool searchCurrentDocument,
                CancellationToken cancellationToken)
            {
                _workspace = workspace;
                _displayFactory = displayFactory;
                _callback = callback;
                _searchPattern = searchPattern;
                _searchCurrentDocument = searchCurrentDocument;
                _cancellationToken = cancellationToken;
                _progress = new ProgressTracker(callback.ReportProgress);
                _asyncListener = asyncListener;

                if (_searchCurrentDocument)
                {
                    var documentService = _workspace.Services.GetService<IDocumentTrackingService>();
                    _currentDocuments = documentService.GetActiveDocuments()
                        .Select(x => _workspace.CurrentDocuments.GetDocument(x))
                        .Where(x => x != null)
                        .ToImmutableArray();
                }
                else
                {
                    _currentDocuments = ImmutableArray<Document>.Empty;
                }
            }

            internal async void Search()
            {
                try
                {
                    //using (var navigateToSearch = Logger.LogBlock(FunctionId.NavigateTo_Search, KeyValueLogMessage.Create(LogType.UserAction), _cancellationToken))
                    using (var asyncToken = _asyncListener.BeginAsyncOperation(GetType() + ".Search"))
                    {
                        if (_searchCurrentDocument)
                        {
                            _progress.AddItems(_currentDocuments.Length);

                            foreach (var currentDocument in _currentDocuments)
                            {
                                await SearchAsync(currentDocument).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            var documents = _workspace.CurrentDocuments.Documents.ToImmutableArray();

                            _progress.AddItems(documents.Length);

                            foreach (var document in documents)
                            {
                                await SearchAsync(document).ConfigureAwait(false);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    _callback.Done();
                }
            }

            private async Task SearchAsync(Document document)
            {
                try
                {
                    await SearchAsyncWorker(document).ConfigureAwait(false);
                }
                finally
                {
                    _progress.ItemCompleted();
                }
            }

            private async Task SearchAsyncWorker(Document document)
            {
                var service = document.Workspace.Services.GetService<INavigateToSearchService>();
                if (service != null)
                {
                    var searchTask = service.SearchDocumentAsync(document, _searchPattern, _cancellationToken);

                    var results = await searchTask.ConfigureAwait(false);
                    if (results != null)
                    {
                        foreach (var result in results)
                        {
                            ReportMatchResult(document, result);
                        }
                    }
                }
            }

            private void ReportMatchResult(Document document, INavigateToSearchResult result)
            {
                var navigateToItem = new NavigateToItem(
                    result.Name,
                    result.Kind,
                    document.Language,
                    result.SecondarySort,
                    result,
                    GetMatchKind(result.MatchKind),
                    result.IsCaseSensitive,
                    _displayFactory);
                _callback.AddItem(navigateToItem);
            }

            private MatchKind GetMatchKind(NavigateToMatchKind matchKind)
            {
                switch (matchKind)
                {
                    case NavigateToMatchKind.Exact: return MatchKind.Exact;
                    case NavigateToMatchKind.Prefix: return MatchKind.Prefix;
                    case NavigateToMatchKind.Substring: return MatchKind.Substring;
                    case NavigateToMatchKind.Regular: return MatchKind.Regular;
                    default: return MatchKind.None;
                }
            }
        }
    }
}
