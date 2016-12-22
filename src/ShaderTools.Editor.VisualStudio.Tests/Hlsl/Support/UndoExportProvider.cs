using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support
{
    // Based partly on https://github.com/jaredpar/EditorUtils/blob/master/Src/EditorUtils/EditorHostFactory.cs

    /// <summary>
    /// In order to host the editor we need to provide an ITextUndoHistory export.  However 
    /// we can't simply export it from the DLL because it would conflict with Visual Studio's
    /// export of ITextUndoHistoryRegistry in the default scenario.  This ComposablePartCatalog
    /// is simply here to hand export the type in the hosted scenario only
    /// </summary>
    internal sealed class UndoExportProvider : ExportProvider
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
    internal interface IBasicUndoHistoryRegistry
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

    internal interface IBasicUndoHistory : ITextUndoHistory
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
}