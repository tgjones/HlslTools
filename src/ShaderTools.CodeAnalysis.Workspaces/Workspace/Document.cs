using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Properties;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Collections;
using ShaderTools.Utilities.Diagnostics;
using ShaderTools.Utilities.ErrorReporting;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis
{
    /// <summary>
    /// Contains the details and contents of an open document.
    /// </summary>
    public sealed class Document
    {
        private readonly HostLanguageServices _languageServices;
        private readonly AsyncLazy<SyntaxTreeBase> _lazySyntaxTree;
        private readonly AsyncLazy<SemanticModelBase> _lazySemanticModel;

        /// <summary>
        /// Gets a unique string that identifies this file.  At this time,
        /// this property returns a normalized version of the value stored
        /// in the FilePath property.
        /// </summary>
        public DocumentId Id { get; }

        public SourceText SourceText { get; }

        /// <summary>
        /// Gets the path at which this file resides.
        /// </summary>
        public string FilePath { get; }

        public string Name => (FilePath != null) ? Path.GetFileName(FilePath) : "[NoName]";

        public string Language => _languageServices.Language;

        public HostLanguageServices LanguageServices => _languageServices;

        public Workspace Workspace => _languageServices.WorkspaceServices.Workspace;

        internal Document(HostLanguageServices languageServices, DocumentId documentId, SourceFile file)
        {
            _languageServices = languageServices;

            Id = documentId;
            SourceText = file.Text;
            FilePath = file.FilePath;

            _lazySyntaxTree = new AsyncLazy<SyntaxTreeBase>(ct => Task.Run(() =>
            {
                var syntaxTreeFactory = languageServices.GetRequiredService<ISyntaxTreeFactoryService>();

                var syntaxTree = syntaxTreeFactory.ParseSyntaxTree(file, ct);

                // make sure there is an association between this tree and this doc id before handing it out
                BindSyntaxTreeToId(syntaxTree, this.Id);

                return syntaxTree;
            }, ct), true);

            _lazySemanticModel = new AsyncLazy<SemanticModelBase>(ct => Task.Run(async () =>
            {
                var syntaxTree = await GetSyntaxTreeAsync(ct).ConfigureAwait(false);

                var compilationFactory = languageServices.GetRequiredService<ICompilationFactoryService>();

                return compilationFactory.CreateCompilation(syntaxTree).GetSemanticModelBase(ct);
            }, ct), true);
        }

        public bool SupportsSemanticModel => LanguageServices.GetService<ICompilationFactoryService>() != null;

        public Task<SyntaxTreeBase> GetSyntaxTreeAsync(CancellationToken cancellationToken)
        {
            return _lazySyntaxTree.GetValueAsync(cancellationToken);
        }

        internal async Task<SyntaxNodeBase> GetSyntaxRootAsync(CancellationToken cancellationToken)
        {
            var syntaxTree = await GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            return syntaxTree.Root;
        }

        /// <summary>
        /// Only for features that absolutely must run synchronously (probably because they're
        /// on the UI thread).  Right now, the only feature this is for is Outlining as VS will
        /// block on that feature from the UI thread when a document is opened.
        /// </summary>
        internal SyntaxNodeBase GetSyntaxRootSynchronously(CancellationToken cancellationToken)
        {
            var tree = this.GetSyntaxTreeSynchronously(cancellationToken);
            return tree.Root;
        }

        internal SyntaxTreeBase GetSyntaxTreeSynchronously(CancellationToken cancellationToken)
        {
            return _lazySyntaxTree.GetValue(cancellationToken);
        }

        /// <summary>
        /// Get the current syntax tree for the document if the text is already loaded and the tree is already parsed.
        /// In almost all cases, you should call <see cref="GetSyntaxTreeAsync"/> to fetch the tree, which will parse the tree
        /// if it's not already parsed.
        /// </summary>
        public bool TryGetSyntaxTree(out SyntaxTreeBase syntaxTree)
        {
            return _lazySyntaxTree.TryGetValue(out syntaxTree);
        }

        public async Task<SemanticModelBase> GetSemanticModelAsync(CancellationToken cancellationToken)
        {
            if (!SupportsSemanticModel)
                return null;

            var options = await GetOptionsAsync(cancellationToken).ConfigureAwait(false);

            if (!options.GetOption(FeatureOnOffOptions.IntelliSense))
                return null;

            return await _lazySemanticModel.GetValueAsync(cancellationToken);
        }

        public Document WithId(DocumentId documentId)
        {
            return new Document(_languageServices, documentId, new SourceFile(SourceText, FilePath));
        }

        /// <summary>
        /// Creates a new instance of this document updated to have the text specified.
        /// </summary>
        public Document WithText(SourceText newText)
        {
            return new Document(_languageServices, Id, new SourceFile(newText, FilePath));
        }

        public Document WithFilePath(string filePath)
        {
            return new Document(_languageServices, Id, new SourceFile(SourceText, filePath));
        }

        /// <summary>
        /// Get the text changes between this document and a prior version of the same document.
        /// The changes, when applied to the text of the old document, will produce the text of the current document.
        /// </summary>
        public async Task<IEnumerable<TextChange>> GetTextChangesAsync(Document oldDocument, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                //using (Logger.LogBlock(FunctionId.Workspace_Document_GetTextChanges, this.Name, cancellationToken))
                {
                    if (oldDocument == this)
                    {
                        // no changes
                        return SpecializedCollections.EmptyEnumerable<TextChange>();
                    }

                    if (this.Id != oldDocument.Id)
                    {
                        throw new ArgumentException(WorkspacesResources.The_specified_document_is_not_a_version_of_this_document);
                    }

                    // first try to see if text already knows its changes
                    IList<TextChange> textChanges = null;
                    var text = this.SourceText;
                    var oldText = oldDocument.SourceText;

                    if (text == oldText)
                    {
                        return SpecializedCollections.EmptyEnumerable<TextChange>();
                    }

                    var container = text.Container;
                    if (container != null)
                    {
                        textChanges = text.GetTextChanges(oldText).ToList();

                        // if changes are significant (not the whole document being replaced) then use these changes
                        if (textChanges.Count > 1 || (textChanges.Count == 1 && textChanges[0].Span != new TextSpan(0, oldText.Length)))
                        {
                            return textChanges;
                        }
                    }

                    return text.GetTextChanges(oldText).ToList();
                }
            }
            catch (Exception e) when (FatalError.ReportUnlessCanceled(e))
            {
                throw ExceptionUtilities.Unreachable;
            }
        }

        private AsyncLazy<DocumentOptionSet> _cachedOptions;

        /// <summary>
        /// Returns the options that should be applied to this document. This consists of global options from <see cref="Solution.Options"/>,
        /// merged with any settings the user has specified at the document levels.
        /// </summary>
        /// <remarks>
        /// This method is async because this may require reading other files. In files that are already open, this is expected to be cheap and complete synchronously.
        /// </remarks>
        public Task<DocumentOptionSet> GetOptionsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_cachedOptions == null)
            {
                var newAsyncLazy = new AsyncLazy<DocumentOptionSet>(async c =>
                {
                    var optionsService = Workspace.Services.GetRequiredService<IOptionService>();
                    var optionSet = await optionsService.GetUpdatedOptionSetForDocumentAsync(this, Workspace.Options, c).ConfigureAwait(false);
                    return new DocumentOptionSet(optionSet, Language);
                }, cacheResult: true);

                Interlocked.CompareExchange(ref _cachedOptions, newAsyncLazy, comparand: null);
            }

            return _cachedOptions.GetValueAsync(cancellationToken);
        }

        private static readonly ReaderWriterLockSlim s_syntaxTreeToIdMapLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private static readonly ConditionalWeakTable<SyntaxTreeBase, DocumentId> s_syntaxTreeToIdMap =
            new ConditionalWeakTable<SyntaxTreeBase, DocumentId>();

        private static void BindSyntaxTreeToId(SyntaxTreeBase tree, DocumentId id)
        {
            using (s_syntaxTreeToIdMapLock.DisposableWrite())
            {
                if (s_syntaxTreeToIdMap.TryGetValue(tree, out var existingId))
                {
                    Contract.ThrowIfFalse(existingId == id);
                }
                else
                {
                    s_syntaxTreeToIdMap.Add(tree, id);
                }
            }
        }

        internal static DocumentId GetDocumentIdForTree(SyntaxTreeBase tree)
        {
            using (s_syntaxTreeToIdMapLock.DisposableRead())
            {
                s_syntaxTreeToIdMap.TryGetValue(tree, out var id);
                return id;
            }
        }
    }
}
