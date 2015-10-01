using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;
using NUnit.Framework;

namespace HlslTools.VisualStudio.Tests
{
    // Based partly on https://github.com/jaredpar/EditorUtils/blob/master/Src/EditorUtils/EditorHostFactory.cs
    internal abstract class MefTestsBase
    {
        private const EditorVersion ReferencedEditorVersion = EditorVersion.Vs2010;

        private static readonly string[] EditorComponents =
        {
            // Core editor components
            "Microsoft.VisualStudio.Platform.VSEditor.dll",

            // Not entirely sure why this is suddenly needed
            "Microsoft.VisualStudio.Text.Internal.dll",

            // Must include this because several editor options are actually stored as exported information 
            // on this DLL.  Including most importantly, the tabsize information
            "Microsoft.VisualStudio.Text.Logic.dll",

            // Include this DLL to get several more EditorOptions including WordWrapStyle
            "Microsoft.VisualStudio.Text.UI.dll",

            // Include this DLL to get more EditorOptions values and the core editor
            "Microsoft.VisualStudio.Text.UI.Wpf.dll"
        };

        /// <summary>
        /// A list of key names for versions of Visual Studio which have the editor components 
        /// necessary to create an EditorHost instance.  Listed in preference order
        /// </summary>
        private static readonly string[] VisualStudioSkuKeyNames =
        {
            // Standard non-express SKU of Visual Studio
            "VisualStudio",

            // Windows Desktop express
            "WDExpress",

            // Visual C# express
            "VCSExpress",

            // Visual C++ express
            "VCExpress",

            // Visual Basic Express
            "VBExpress",
        };

        /// <summary>
        /// The minimum <see cref="EditorVersion"/> value supported by this assembly. 
        /// </summary>
        public static EditorVersion MinimumEditorVersion
        {
            get { return ReferencedEditorVersion; }
        }

        public static EditorVersion MaxEditorVersion
        {
            get { return EditorVersion.Vs2015; }
        }

        protected CompositionContainer Container { get; private set; }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var editorCatalogs = GetEditorCatalogs(null);
            var localCatalog = new DirectoryCatalog(".");
            var catalog = new AggregateCatalog(editorCatalogs.Union(new[] { localCatalog }));
            Container = new CompositionContainer(catalog, new UndoExportProvider());

            OnTestFixtureSetUp();
        }

        /// <summary>
        /// Load the list of editor assemblies into the specified catalog list.  This method will
        /// throw on failure
        /// </summary>
        private static IEnumerable<ComposablePartCatalog> GetEditorCatalogs(EditorVersion? editorVersion)
        {
            string version;
            string installDirectory;
            if (!TryGetEditorInfo(editorVersion, out version, out installDirectory))
            {
                throw new Exception("Unable to calculate the version of Visual Studio installed on the machine");
            }

            if (!TryLoadInteropAssembly(installDirectory))
            {
                var message = string.Format("Unable to load the interop assemblies.  Install directory is: ", installDirectory);
                throw new Exception(message);
            }

            // Load the core editor compontents from the GAC
            var versionInfo = string.Format(", Version={0}, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL", version);
            foreach (var name in EditorComponents)
            {
                var simpleName = name.Substring(0, name.Length - 4);
                var qualifiedName = simpleName + versionInfo;

                Assembly assembly;
                try
                {
                    assembly = Assembly.Load(qualifiedName);
                }
                catch (Exception e)
                {
                    var msg = string.Format("Unable to load editor dependency {0}", name);
                    throw new Exception(msg, e);
                }

                yield return new AssemblyCatalog(assembly);
            }
        }

        private static bool TryGetEditorInfo(EditorVersion? editorVersion, out string fullVersion, out string installDirectory)
        {
            if (editorVersion.HasValue)
            {
                var shortVersion = GetShortVersionString(editorVersion.Value);
                return TryGetEditorInfoCore(shortVersion, out fullVersion, out installDirectory);
            }

            return TryCalculateEditorInfo(out fullVersion, out installDirectory);
        }

        /// <summary>
        /// Try and calculate the version of Visual Studio installed on this machine.  Need both the version
        /// and the install directory in order to load up the editor components for testing
        /// </summary>
        private static bool TryCalculateEditorInfo(out string fullVersion, out string installDirectory)
        {
            // The same pattern exists for all known versions of Visual Studio.  The editor was 
            // introduced in version 10 (VS2010).  The max of 20 is arbitrary and just meant to 
            // future proof this algorithm for some time into the future
            var max = GetVersionNumber(MaxEditorVersion);
            for (int i = GetVersionNumber(MinimumEditorVersion); i <= max; i++)
            {
                var shortVersion = String.Format("{0}.0", i);
                if (TryGetEditorInfoCore(shortVersion, out fullVersion, out installDirectory))
                {
                    return true;
                }
            }

            installDirectory = null;
            fullVersion = null;
            return false;
        }

        private static bool TryGetEditorInfoCore(string shortVersion, out string fullversion, out string installDirectory)
        {
            if (TryGetInstallDirectory(shortVersion, out installDirectory))
            {
                fullversion = string.Format("{0}.0.0", shortVersion);
                return true;
            }

            fullversion = null;
            return false;
        }

        /// <summary>
        /// Try and get the installation directory for the specified version of Visual Studio.  This 
        /// will fail if the specified version of Visual Studio isn't installed
        /// </summary>
        private static bool TryGetInstallDirectory(string shortVersion, out string installDirectory)
        {
            foreach (var skuKeyName in VisualStudioSkuKeyNames)
            {
                if (TryGetInstallDirectory(skuKeyName, shortVersion, out installDirectory))
                {
                    return true;
                }
            }

            installDirectory = null;
            return false;
        }

        /// <summary>
        /// Try and get the installation directory for the specified SKU of Visual Studio.  This 
        /// will fail if the specified version of Visual Studio isn't installed
        /// </summary>
        private static bool TryGetInstallDirectory(string skuKeyName, string shortVersion, out string installDirectory)
        {
            try
            {
                var subKeyPath = String.Format(@"Software\Microsoft\{0}\{1}", skuKeyName, shortVersion);
                using (var key = Registry.LocalMachine.OpenSubKey(subKeyPath, writable: false))
                {
                    if (key != null)
                    {
                        installDirectory = key.GetValue("InstallDir", null) as string;
                        if (!String.IsNullOrEmpty(installDirectory))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore and try the next version
            }

            installDirectory = null;
            return false;
        }

        /// <summary>
        /// The interop assembly isn't included in the GAC and it doesn't offer any MEF components (it's
        /// just a simple COM interop library).  Hence it needs to be loaded a bit specially.  Just find
        /// the assembly on disk and hook into the resolve event
        /// </summary>
        private static bool TryLoadInteropAssembly(string installDirectory)
        {
            const string interopName = "Microsoft.VisualStudio.Platform.VSEditor.Interop";
            const string interopNameWithExtension = interopName + ".dll";
            var interopAssemblyPath = Path.Combine(installDirectory, "PrivateAssemblies");
            interopAssemblyPath = Path.Combine(interopAssemblyPath, interopNameWithExtension);
            try
            {
                var interopAssembly = Assembly.LoadFrom(interopAssemblyPath);
                if (interopAssembly == null)
                {
                    return false;
                }

                var comparer = StringComparer.OrdinalIgnoreCase;
                AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
                {
                    if (comparer.Equals(e.Name, interopAssembly.FullName))
                    {
                        return interopAssembly;
                    }

                    return null;
                };

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static int GetVersionNumber(EditorVersion version)
        {
            switch (version)
            {
                case EditorVersion.Vs2010: return 10;
                case EditorVersion.Vs2012: return 11;
                case EditorVersion.Vs2013: return 12;
                case EditorVersion.Vs2015: return 14;
                default:
                    throw new Exception(string.Format("Unexpected enum value {0}", version));
            }
        }

        internal static string GetShortVersionString(EditorVersion version)
        {
            var number = GetVersionNumber(version);
            return string.Format("{0}.0", number);
        }

        /// <summary>
        /// The supported list of editor versions 
        /// </summary>
        /// <remarks>These must be listed in ascending version order</remarks>
        public enum EditorVersion
        {
            Vs2010,
            Vs2012,
            Vs2013,
            Vs2015,
        }

        /// <summary>
        /// In order to host the editor we need to provide an ITextUndoHistory export.  However 
        /// we can't simply export it from the DLL because it would conflict with Visual Studio's
        /// export of ITextUndoHistoryRegistry in the default scenario.  This ComposablePartCatalog
        /// is simply here to hand export the type in the hosted scenario only
        /// </summary>
        private sealed class UndoExportProvider : ExportProvider
        {
            private readonly IBasicUndoHistoryRegistry _basicUndoHistoryRegistry;
            private readonly string _textUndoHistoryRegistryContractName;
            private readonly string _basicUndoHistoryRegistryContractName;
            private readonly Export _export;

            internal UndoExportProvider()
            {
                _textUndoHistoryRegistryContractName = AttributedModelServices.GetContractName(typeof(ITextUndoHistoryRegistry));
                _basicUndoHistoryRegistryContractName = AttributedModelServices.GetContractName(typeof(IBasicUndoHistoryRegistry));
                _basicUndoHistoryRegistry = new BasicTextUndoHistoryRegistry();
                _export = new Export(_textUndoHistoryRegistryContractName, () => _basicUndoHistoryRegistry);
            }

            protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
            {
                if (definition.ContractName == _textUndoHistoryRegistryContractName ||
                    definition.ContractName == _basicUndoHistoryRegistryContractName)
                {
                    yield return _export;
                }
            }
        }

        /// <summary>
        /// In certain hosted scenarios the default ITextUndoHistoryRegistry won't be 
        /// available.  This is a necessary part of editor composition though and some 
        /// implementation needs to be provided.  Importing this type will provide a 
        /// very basic implementation
        ///
        /// This type intentionally doesn't ever export ITextUndoHistoryRegistry.  Doing
        /// this would conflict with Visual Studios export and cause a MEF composition 
        /// error.  It's instead exposed via this interface 
        ///
        /// In general this type won't be used except in testing
        /// </summary>
        public interface IBasicUndoHistoryRegistry
        {
            /// <summary>
            /// Get the basic implementation of the ITextUndoHistoryRegistry
            /// </summary>
            ITextUndoHistoryRegistry TextUndoHistoryRegistry { get; }

            /// <summary>
            /// Try and get the IBasicUndoHistory for the given context
            /// </summary>
            bool TryGetBasicUndoHistory(object context, out IBasicUndoHistory basicUndoHistory);
        }

        public interface IBasicUndoHistory : ITextUndoHistory
        {
            /// <summary>
            /// Clear out all of the state including the undo and redo stacks
            /// </summary>
            void Clear();
        }

        /// <summary>
        /// This class is intended to be a very simple ITextUndoHistoryRegistry implementation for hosts that
        /// don't have a built-in undo mechanism
        /// </summary>
        internal sealed class BasicTextUndoHistoryRegistry : ITextUndoHistoryRegistry, IBasicUndoHistoryRegistry
        {
            private readonly ConditionalWeakTable<object, IBasicUndoHistory> _map = new ConditionalWeakTable<object, IBasicUndoHistory>();

            internal BasicTextUndoHistoryRegistry()
            {

            }

            private bool TryGetHistory(object context, out IBasicUndoHistory basicUndoHistory)
            {
                return _map.TryGetValue(context, out basicUndoHistory);
            }

            #region ITextUndoHistoryRegistry

            /// <summary>
            /// Easy to implement but the Visual Studio implementation throws a NotSupportedException
            /// </summary>
            void ITextUndoHistoryRegistry.AttachHistory(object context, ITextUndoHistory history)
            {
                throw new NotSupportedException();
            }

            ITextUndoHistory ITextUndoHistoryRegistry.GetHistory(object context)
            {
                IBasicUndoHistory history;
                _map.TryGetValue(context, out history);
                return history;
            }

            ITextUndoHistory ITextUndoHistoryRegistry.RegisterHistory(object context)
            {
                IBasicUndoHistory history;
                if (!_map.TryGetValue(context, out history))
                {
                    history = new BasicUndoHistory(context);
                    _map.Add(context, history);
                }
                return history;
            }

            void ITextUndoHistoryRegistry.RemoveHistory(ITextUndoHistory history)
            {
                var basicUndoHistory = history as BasicUndoHistory;
                if (basicUndoHistory != null)
                {
                    _map.Remove(basicUndoHistory.Context);
                    basicUndoHistory.Clear();
                }
            }

            bool ITextUndoHistoryRegistry.TryGetHistory(object context, out ITextUndoHistory history)
            {
                IBasicUndoHistory basicUndoHistory;
                if (TryGetHistory(context, out basicUndoHistory))
                {
                    history = basicUndoHistory;
                    return true;
                }

                history = null;
                return false;
            }

            #endregion

            #region IBasciUndoHistoryRegistry

            ITextUndoHistoryRegistry IBasicUndoHistoryRegistry.TextUndoHistoryRegistry
            {
                get { return this; }
            }

            bool IBasicUndoHistoryRegistry.TryGetBasicUndoHistory(object context, out IBasicUndoHistory basicUndoHistory)
            {
                return TryGetHistory(context, out basicUndoHistory);
            }

            #endregion
        }

        /// <summary>
        /// Provides a very simple ITextUndoHistory implementation.  Sufficient for us to test
        /// simple undo scenarios inside the unit tests
        /// </summary>
        internal sealed class BasicUndoHistory : IBasicUndoHistory
        {
            private readonly object _context;
            private readonly Stack<BasicUndoTransaction> _openTransactionStack = new Stack<BasicUndoTransaction>();
            private readonly Stack<ITextUndoTransaction> _undoStack = new Stack<ITextUndoTransaction>();
            private readonly Stack<ITextUndoTransaction> _redoStack = new Stack<ITextUndoTransaction>();
            private readonly PropertyCollection _properties = new PropertyCollection();
            private TextUndoHistoryState _state = TextUndoHistoryState.Idle;
            private event EventHandler<TextUndoRedoEventArgs> _undoRedoHappened;
            private event EventHandler<TextUndoTransactionCompletedEventArgs> _undoTransactionCompleted;

            internal ITextUndoTransaction CurrentTransaction
            {
                get { return _openTransactionStack.Count > 0 ? _openTransactionStack.Peek() : null; }
            }

            internal Stack<ITextUndoTransaction> UndoStack
            {
                get { return _undoStack; }
            }

            internal Stack<ITextUndoTransaction> RedoStack
            {
                get { return _redoStack; }
            }

            internal object Context
            {
                get { return _context; }
            }

            internal BasicUndoHistory(object context)
            {
                _context = context;
            }

            internal ITextUndoTransaction CreateTransaction(string description)
            {
                _openTransactionStack.Push(new BasicUndoTransaction(this, description));
                return _openTransactionStack.Peek();
            }

            internal void Redo(int count)
            {
                try
                {
                    count = Math.Min(_redoStack.Count, count);
                    _state = TextUndoHistoryState.Redoing;
                    for (var i = 0; i < count; i++)
                    {
                        var current = _redoStack.Peek();
                        current.Do();
                        _redoStack.Pop();
                        _undoStack.Push(current);
                    }

                    RaiseUndoRedoHappened();
                }
                finally
                {
                    _state = TextUndoHistoryState.Idle;
                }
            }

            internal void Undo(int count)
            {
                try
                {
                    count = Math.Min(_undoStack.Count, count);
                    _state = TextUndoHistoryState.Undoing;
                    for (var i = 0; i < count; i++)
                    {
                        var current = _undoStack.Peek();
                        current.Undo();
                        _undoStack.Pop();
                        _redoStack.Push(current);
                    }

                    RaiseUndoRedoHappened();
                }
                finally
                {
                    _state = TextUndoHistoryState.Idle;
                }
            }

            internal void OnTransactionClosed(BasicUndoTransaction transaction, bool didComplete)
            {
                if (_openTransactionStack.Count == 0 || transaction != _openTransactionStack.Peek())
                {
                    // Happens in dispose after complete / cancel
                    return;
                }

                _openTransactionStack.Pop();
                if (!didComplete)
                {
                    return;
                }

                if (_openTransactionStack.Count == 0)
                {
                    _undoStack.Push(transaction);
                    var list = _undoTransactionCompleted;
                    if (list != null)
                    {
                        list(this, new TextUndoTransactionCompletedEventArgs(null, TextUndoTransactionCompletionResult.TransactionAdded));
                    }
                }
                else
                {
                    foreach (var cur in transaction.UndoPrimitives)
                    {
                        _openTransactionStack.Peek().UndoPrimitives.Add(cur);
                    }
                }
            }

            internal void Clear()
            {
                if (_state != TextUndoHistoryState.Idle || CurrentTransaction != null)
                {
                    throw new InvalidOperationException("Can't clear with an open transaction or in undo / redo");
                }

                _undoStack.Clear();
                _redoStack.Clear();

                // The IEditorOperations AddAfterTextBufferChangePrimitive and AddBeforeTextBufferChangePrimitive
                // implementations store an ITextView in the Property of the associated ITextUndoHistory.  It's
                // necessary to keep this value present so long as the primitives are in the undo / redo stack
                // as their implementation depends on it.  Once the stack is cleared we can safely remove 
                // the value.
                //
                // This is in fact necessary for sane testing.  Without this removal it's impossible to have 
                // an ITextView disconnect and be collected from it's underlying ITextBuffer.  The ITextUndoHistory
                // is associated with an ITextBuffer and through it's undo stack will keep the ITextView alive
                // indefinitely
                _properties.RemoveProperty(typeof(ITextView));
            }

            private void RaiseUndoRedoHappened()
            {
                var list = _undoRedoHappened;
                if (list != null)
                {
                    // Note: Passing null here as this is what Visual Studio does
                    list(this, new TextUndoRedoEventArgs(_state, null));
                }
            }

            #region ITextUndoHistory

            /// <summary>
            /// Return 'true' here instead of actually looking at the redo stack count because
            /// this is the behavior of the standard Visual Studio undo manager
            /// </summary>
            bool ITextUndoHistory.CanRedo
            {
                get { return true; }
            }

            /// <summary>
            /// Return 'true' here instead of actually looking at the redo stack count because
            /// this is the behavior of the standard Visual Studio undo manager
            /// </summary>
            bool ITextUndoHistory.CanUndo
            {
                get { return true; }
            }

            ITextUndoTransaction ITextUndoHistory.CreateTransaction(string description)
            {
                return CreateTransaction(description);
            }

            ITextUndoTransaction ITextUndoHistory.CurrentTransaction
            {
                get { return CurrentTransaction; }
            }

            /// <summary>
            /// Easy to implement but not supported by Visual Studio
            /// </summary>
            ITextUndoTransaction ITextUndoHistory.LastRedoTransaction
            {
                get { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Easy to implement but not supported by Visual Studio
            /// </summary>
            ITextUndoTransaction ITextUndoHistory.LastUndoTransaction
            {
                get { throw new NotSupportedException(); }
            }

            void ITextUndoHistory.Redo(int count)
            {
                Redo(count);
            }

            /// <summary>
            /// Easy to implement but not supported by Visual Studio
            /// </summary>
            string ITextUndoHistory.RedoDescription
            {
                get { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Easy to implement but not supported by Visual Studio
            /// </summary>
            IEnumerable<ITextUndoTransaction> ITextUndoHistory.RedoStack
            {
                get { throw new NotSupportedException(); }
            }

            TextUndoHistoryState ITextUndoHistory.State
            {
                [DebuggerNonUserCode]
                get
                { return _state; }
            }

            void ITextUndoHistory.Undo(int count)
            {
                Undo(count);
            }

            /// <summary>
            /// Easy to implement but not supported by Visual Studio
            /// </summary>
            string ITextUndoHistory.UndoDescription
            {
                get { throw new NotSupportedException(); }
            }

            event EventHandler<TextUndoRedoEventArgs> ITextUndoHistory.UndoRedoHappened
            {
                add { _undoRedoHappened += value; }
                remove { _undoRedoHappened -= value; }
            }

            /// <summary>
            /// Easy to implement but not supported by Visual Studio
            /// </summary>
            IEnumerable<ITextUndoTransaction> ITextUndoHistory.UndoStack
            {
                get { throw new NotSupportedException(); }
            }

            event EventHandler<TextUndoTransactionCompletedEventArgs> ITextUndoHistory.UndoTransactionCompleted
            {
                add { _undoTransactionCompleted += value; }
                remove { _undoTransactionCompleted -= value; }
            }

            PropertyCollection IPropertyOwner.Properties
            {
                get { return _properties; }
            }

            #endregion

            #region IBasicUndoHistory

            void IBasicUndoHistory.Clear()
            {
                Clear();
            }

            #endregion
        }

        internal sealed class BasicUndoTransaction : ITextUndoTransaction
        {
            private readonly BasicUndoHistory _textUndoHistory;
            private readonly List<ITextUndoPrimitive> _primitiveList = new List<ITextUndoPrimitive>();

            internal string Description
            {
                get;
                set;
            }

            internal List<ITextUndoPrimitive> UndoPrimitives
            {
                get { return _primitiveList; }
            }

            internal BasicUndoTransaction(BasicUndoHistory textUndoHistory, string description)
            {
                _textUndoHistory = textUndoHistory;
                Description = description;
            }

            #region ITextUndoTransaction

            void ITextUndoTransaction.AddUndo(ITextUndoPrimitive undo)
            {
                _primitiveList.Add(undo);
            }

            /// <summary>
            /// Visual Studio implementation throw so duplicate here
            /// </summary>
            bool ITextUndoTransaction.CanRedo
            {
                get { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Visual Studio implementation throw so duplicate here
            /// </summary>
            bool ITextUndoTransaction.CanUndo
            {
                get { throw new NotSupportedException(); }
            }

            ITextUndoHistory ITextUndoTransaction.History
            {
                get { return _textUndoHistory; }
            }

            IMergeTextUndoTransactionPolicy ITextUndoTransaction.MergePolicy
            {
                get;
                set;
            }

            ITextUndoTransaction ITextUndoTransaction.Parent
            {
                get { throw new NotSupportedException(); }
            }

            IList<ITextUndoPrimitive> ITextUndoTransaction.UndoPrimitives
            {
                get { return UndoPrimitives; }
            }

            UndoTransactionState ITextUndoTransaction.State
            {
                get { throw new NotSupportedException(); }
            }

            string ITextUndoTransaction.Description
            {
                get { return Description; }
                set { Description = value; }
            }

            void ITextUndoTransaction.Cancel()
            {
                _textUndoHistory.OnTransactionClosed(this, didComplete: false);
            }

            void ITextUndoTransaction.Complete()
            {
                _textUndoHistory.OnTransactionClosed(this, didComplete: true);
            }

            void ITextUndoTransaction.Do()
            {
                for (var i = 0; i < _primitiveList.Count; i++)
                {
                    _primitiveList[i].Do();
                }
            }

            void ITextUndoTransaction.Undo()
            {
                for (var i = _primitiveList.Count - 1; i >= 0; i--)
                {
                    _primitiveList[i].Undo();
                }
            }

            #endregion

            #region IDisposable

            void IDisposable.Dispose()
            {
                _textUndoHistory.OnTransactionClosed(this, didComplete: false);
            }

            #endregion
        }

        protected virtual void OnTestFixtureSetUp()
        {

        }
    }
}