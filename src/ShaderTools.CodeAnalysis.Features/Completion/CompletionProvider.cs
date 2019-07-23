// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.CodeAnalysis.Completion
{
    /// <summary>
    /// Implement a subtype of this class and export it to provide completions during typing in an editor.
    /// </summary>
    internal abstract class CompletionProvider
    {
        internal string Name { get; }

        protected CompletionProvider()
        {
            this.Name = this.GetType().FullName;
        }

        /// <summary>
        /// Implement to contribute <see cref="CompletionItem"/>'s and other details to a <see cref="CompletionList"/>
        /// </summary>
        public abstract Task ProvideCompletionsAsync(CompletionContext context);

        /// <summary>
        /// Returns true if the character recently inserted or deleted in the text should trigger completion.
        /// </summary>
        /// <param name="text">The text that completion is occuring within.</param>
        /// <param name="caretPosition">The position of the caret after the triggering action.</param>
        /// <param name="trigger">The triggering action.</param>
        /// <param name="options">The set of options in effect.</param>
        public virtual bool ShouldTriggerCompletion(SourceText text, int position, CompletionTrigger trigger, OptionSet options)
        {
            switch (trigger.Kind)
            {
                case CompletionTriggerKind.Insertion:
                    var insertedCharacterPosition = position - 1;
                    return this.IsInsertionTrigger(text, insertedCharacterPosition, options);

                default:
                    return false;
            }
        }

        internal virtual bool IsInsertionTrigger(SourceText text, int insertedCharacterPosition, OptionSet options)
        {
            return false;
        }
    }
}