// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Completion
{
    /// <summary>
    /// The change to be applied to the document when a <see cref="CompletionItem"/> is committed.
    /// </summary>
    public sealed class CompletionChange
    {
        /// <summary>
        /// The text change to be applied to the document.
        /// </summary>
        public TextChange TextChange { get; }

        /// <summary>
        /// The new caret position after the change has been applied.
        /// If null then the new caret position will be determined by the completion host.
        /// </summary>
        public int? NewPosition { get; }

        /// <summary>
        /// True if the changes include the typed character that caused the <see cref="CompletionItem"/>
        /// to be committed.  If false the completion host will determine if and where the commit 
        /// character is inserted into the document.
        /// </summary>
        public bool IncludesCommitCharacter { get; }

        private CompletionChange(TextChange textChange, int? newPosition, bool includesCommitCharacter)
        {
            TextChange = textChange;
            NewPosition = newPosition;
            IncludesCommitCharacter = includesCommitCharacter;
        }

        public static CompletionChange Create(
            TextChange textChange,
            int? newPosition = null,
            bool includesCommitCharacter = false)
        {
            return new CompletionChange(textChange, newPosition, includesCommitCharacter);
        }

        public CompletionChange WithTextChange(TextChange textChange)
        {
            return new CompletionChange(textChange, this.NewPosition, this.IncludesCommitCharacter);
        }

        /// <summary>
        /// Creates a copy of this <see cref="CompletionChange"/> with the <see cref="NewPosition"/> property changed.
        /// </summary>
        public CompletionChange WithNewPosition(int? newPostion)
        {
            return new CompletionChange(this.TextChange, newPostion, this.IncludesCommitCharacter);
        }

        /// <summary>
        /// Creates a copy of this <see cref="CompletionChange"/> with the <see cref="IncludesCommitCharacter"/> property changed.
        /// </summary>
        public CompletionChange WithIncludesCommitCharacter(bool includesCommitCharacter)
        {
            return new CompletionChange(this.TextChange, this.NewPosition, includesCommitCharacter);
        }
    }
}