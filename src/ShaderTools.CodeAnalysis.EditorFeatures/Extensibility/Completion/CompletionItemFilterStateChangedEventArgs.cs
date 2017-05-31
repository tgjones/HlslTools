using System;
using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Completion;

namespace ShaderTools.CodeAnalysis.Editor
{
    internal class CompletionItemFilterStateChangedEventArgs : EventArgs
    {
        public ImmutableDictionary<CompletionItemFilter, bool> FilterState { get; }

        public CompletionItemFilterStateChangedEventArgs(
            ImmutableDictionary<CompletionItemFilter, bool> filterState)
        {
            this.FilterState = filterState;
        }
    }
}