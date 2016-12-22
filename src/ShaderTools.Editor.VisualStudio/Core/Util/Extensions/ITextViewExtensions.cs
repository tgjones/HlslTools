// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace ShaderTools.Editor.VisualStudio.Core.Util.Extensions
{
    internal static partial class ITextViewExtensions
    {
        /// <summary>
        /// Gets or creates a per subject buffer property.
        /// </summary>
        public static TProperty GetOrCreatePerSubjectBufferProperty<TProperty, TTextView>(
            this TTextView textView,
            ITextBuffer subjectBuffer,
            object key,
            Func<TTextView, ITextBuffer, TProperty> valueCreator) where TTextView : class, ITextView
        {
            GetOrCreatePerSubjectBufferProperty(textView, subjectBuffer, key, valueCreator, out var value);

            return value;
        }

        /// <summary>
        /// Gets or creates a per subject buffer property, returning true if it needed to create it.
        /// </summary>
        public static bool GetOrCreatePerSubjectBufferProperty<TProperty, TTextView>(
            this TTextView textView,
            ITextBuffer subjectBuffer,
            object key,
            Func<TTextView, ITextBuffer, TProperty> valueCreator,
            out TProperty value) where TTextView : class, ITextView
        {
            Contract.Requires(textView != null);
            Contract.Requires(subjectBuffer != null);
            Contract.Requires(valueCreator != null);

            return PerSubjectBufferProperty<TProperty, TTextView>.GetOrCreateValue(textView, subjectBuffer, key, valueCreator, out value);
        }
    }
}