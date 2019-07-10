// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.FindUsages
{
    /// <summary>
    /// Information about a symbol's definition that can be displayed in an editor
    /// and used for navigation.
    /// 
    /// Standard implmentations can be obtained through the various <see cref="DefinitionItem"/>.Create
    /// overloads.
    /// 
    /// Subclassing is also supported for scenarios that fall outside the bounds of
    /// these common cases.
    /// </summary>
    internal abstract partial class DefinitionItem
    {
        /// <summary>
        /// Descriptive tags from <see cref="CompletionTags"/>. These tags may influence how the 
        /// item is displayed.
        /// </summary>
        public ImmutableArray<string> Tags { get; }

        /// <summary>
        /// The DisplayParts just for the name of this definition.  Generally used only for 
        /// error messages.
        /// </summary>
        public ImmutableArray<TaggedText> NameDisplayParts { get; }

        /// <summary>
        /// The full display parts for this definition.  Displayed in a classified 
        /// manner when possible.
        /// </summary>
        public ImmutableArray<TaggedText> DisplayParts { get; }

        /// <summary>
        /// Where the location originally came from (for example, the containing assembly or
        /// project name).  May be used in the presentation of a definition.
        /// </summary>
        public ImmutableArray<TaggedText> OriginationParts { get; }

        /// <summary>
        /// Additional locations to present in the UI.  A definition may have multiple locations 
        /// for cases like partial types/members.
        /// </summary>
        public ImmutableArray<DocumentSpan> SourceSpans { get; }

        /// <summary>
        /// Whether or not this definition should be presented if we never found any references to
        /// it.  For example, when searching for a property, the FindReferences engine will cascade
        /// to the accessors in case any code specifically called those accessors (can happen in 
        /// cross-language cases).  However, in the normal case where there were no calls specifically
        /// to the accessor, we would not want to display them in the UI.  
        /// 
        /// For most definitions we will want to display them, even if no references were found.  
        /// This property allows for this customization in behavior.
        /// </summary>
        public bool DisplayIfNoReferences { get; }

        internal abstract bool IsExternal { get; }

        protected DefinitionItem(
            ImmutableArray<string> tags,
            ImmutableArray<TaggedText> displayParts,
            ImmutableArray<TaggedText> nameDisplayParts,
            ImmutableArray<TaggedText> originationParts = default(ImmutableArray<TaggedText>),
            ImmutableArray<DocumentSpan> sourceSpans = default(ImmutableArray<DocumentSpan>),
            bool displayIfNoReferences = true)
        {
            Tags = tags;
            DisplayParts = displayParts;
            NameDisplayParts = nameDisplayParts.IsDefaultOrEmpty ? displayParts : nameDisplayParts;
            OriginationParts = originationParts.NullToEmpty();
            SourceSpans = sourceSpans.NullToEmpty();
            DisplayIfNoReferences = displayIfNoReferences;
        }

        public abstract bool CanNavigateTo();
        public abstract bool TryNavigateTo();

        public static DefinitionItem Create(
            ImmutableArray<string> tags,
            ImmutableArray<TaggedText> displayParts,
            DocumentSpan sourceSpan,
            ImmutableArray<TaggedText> nameDisplayParts = default(ImmutableArray<TaggedText>),
            bool displayIfNoReferences = true)
        {
            return Create(
                tags, displayParts, ImmutableArray.Create(sourceSpan),
                nameDisplayParts, displayIfNoReferences);
        }

        public static DefinitionItem Create(
            ImmutableArray<string> tags,
            ImmutableArray<TaggedText> displayParts,
            ImmutableArray<DocumentSpan> sourceSpans,
            ImmutableArray<TaggedText> nameDisplayParts = default(ImmutableArray<TaggedText>),
            bool displayIfNoReferences = true)
        {
            if (sourceSpans.Length == 0)
            {
                throw new ArgumentException($"{nameof(sourceSpans)} cannot be empty.");
            }

            return new DocumentLocationDefinitionItem(
                tags, displayParts, nameDisplayParts, sourceSpans, displayIfNoReferences);
        }
    }
}