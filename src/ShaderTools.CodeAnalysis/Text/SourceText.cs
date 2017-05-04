// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ShaderTools.CodeAnalysis.Properties;
using ShaderTools.Utilities.Collections;
using ShaderTools.Utilities.PooledObjects;

namespace ShaderTools.CodeAnalysis.Text
{
    /// <summary>
    /// An abstraction of source text.
    /// </summary>
    public abstract class SourceText
    {
        private const int CharBufferSize = 32 * 1024;
        private const int CharBufferCount = 5;
        internal const int LargeObjectHeapLimitInChars = 40 * 1024; // 40KB

        private static readonly ObjectPool<char[]> s_charArrayPool = new ObjectPool<char[]>(() => new char[CharBufferSize], CharBufferCount);

        private SourceTextContainer _lazyContainer;
        private TextLineCollection _lazyLineInfo;

        protected SourceText(SourceTextContainer container = null)
        {
            _lazyContainer = container;
        }

        /// <summary>
        /// Constructs a <see cref="SourceText"/> from text in a string.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="filePath">Fully qualified path to the file that this text was loaded from, if any.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        public static SourceText From(string text, string filePath = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return new StringText(text, filePath);
        }

        /// <summary>
        /// Fully qualified path to the file that this <see cref="SourceText"/> was created from, if any.
        /// </summary>
        public abstract string FilePath { get; }

        /// <summary>
        /// The length of the text in characters.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// The size of the storage representation of the text (in characters).
        /// This can differ from length when storage buffers are reused to represent fragments/subtext.
        /// </summary>
        internal virtual int StorageSize
        {
            get { return this.Length; }
        }

        internal virtual ImmutableArray<SourceText> Segments
        {
            get { return ImmutableArray<SourceText>.Empty; }
        }

        internal virtual SourceText StorageKey
        {
            get { return this; }
        }

        /// <summary>
        /// Returns a character at given position.
        /// </summary>
        /// <param name="position">The position to get the character from.</param>
        /// <returns>The character.</returns>
        /// <exception cref="ArgumentOutOfRangeException">When position is negative or 
        /// greater than <see cref="Length"/>.</exception>
        public abstract char this[int position] { get; }

        /// <summary>
        /// Copy a range of characters from this SourceText to a destination array.
        /// </summary>
        public abstract void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count);

        /// <summary>
        /// The container of this <see cref="SourceText"/>.
        /// </summary>
        public virtual SourceTextContainer Container
        {
            get
            {
                if (_lazyContainer == null)
                {
                    Interlocked.CompareExchange(ref _lazyContainer, new StaticContainer(this), null);
                }

                return _lazyContainer;
            }
        }

        internal void CheckSubSpan(TextSpan span)
        {
            if (span.Start < 0 || span.Start > this.Length || span.End > this.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(span));
            }
        }

        /// <summary>
        /// Gets a <see cref="SourceText"/> that contains the characters in the specified span of this text.
        /// </summary>
        public virtual SourceText GetSubText(TextSpan span)
        {
            CheckSubSpan(span);

            int spanLength = span.Length;
            if (spanLength == 0)
            {
                return SourceText.From(string.Empty);
            }
            else if (spanLength == this.Length && span.Start == 0)
            {
                return this;
            }
            else
            {
                return new SubText(this, span);
            }
        }

        /// <summary>
        /// Write this <see cref="SourceText"/> to a text writer.
        /// </summary>
        public void Write(TextWriter textWriter, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.Write(textWriter, new TextSpan(0, this.Length), cancellationToken);
        }

        /// <summary>
        /// Write a span of text to a text writer.
        /// </summary>
        public virtual void Write(TextWriter writer, TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckSubSpan(span);

            var buffer = s_charArrayPool.Allocate();
            try
            {
                int offset = Math.Min(this.Length, span.Start);
                int length = Math.Min(this.Length, span.End) - offset;
                while (offset < length)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    int count = Math.Min(buffer.Length, length - offset);
                    this.CopyTo(offset, buffer, 0, count);
                    writer.Write(buffer, 0, count);
                    offset += count;
                }
            }
            finally
            {
                s_charArrayPool.Free(buffer);
            }
        }

        /// <summary>
        /// Provides a string representation of the SourceText.
        /// </summary>
        public override string ToString()
        {
            return ToString(new TextSpan(0, this.Length));
        }

        /// <summary>
        /// Gets a string containing the characters in specified span.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When given span is outside of the text range.</exception>
        public virtual string ToString(TextSpan span)
        {
            CheckSubSpan(span);

            // default implementation constructs text using CopyTo
            var builder = new StringBuilder();
            var buffer = new char[Math.Min(span.Length, 1024)];

            int position = Math.Max(Math.Min(span.Start, this.Length), 0);
            int length = Math.Min(span.End, this.Length) - position;

            while (position < this.Length && length > 0)
            {
                int copyLength = Math.Min(buffer.Length, length);
                this.CopyTo(position, buffer, 0, copyLength);
                builder.Append(buffer, 0, copyLength);
                length -= copyLength;
                position += copyLength;
            }

            return builder.ToString();
        }

        #region Changes

        /// <summary>
        /// Constructs a new SourceText from this text with the specified changes.
        /// </summary>
        public virtual SourceText WithChanges(IEnumerable<TextChange> changes)
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            if (!changes.Any())
            {
                return this;
            }

            var segments = ArrayBuilder<SourceText>.GetInstance();
            var changeRanges = ArrayBuilder<TextChangeRange>.GetInstance();
            int position = 0;

            foreach (var change in changes)
            {
                // there can be no overlapping changes
                if (change.Span.Start < position)
                {
                    throw new ArgumentException(CodeAnalysisResources.ChangesMustBeOrderedAndNotOverlapping, nameof(changes));
                }

                var newTextLength = change.NewText?.Length ?? 0;

                // ignore changes that don't change anything
                if (change.Span.Length == 0 && newTextLength == 0)
                    continue;

                // if we've skipped a range, add
                if (change.Span.Start > position)
                {
                    var subText = this.GetSubText(new TextSpan(position, change.Span.Start - position));
                    CompositeText.AddSegments(segments, subText);
                }

                if (newTextLength > 0)
                {
                    var segment = SourceText.From(change.NewText);
                    CompositeText.AddSegments(segments, segment);
                }

                position = change.Span.End;

                changeRanges.Add(new TextChangeRange(change.Span, newTextLength));
            }

            // no changes actually happend?
            if (position == 0 && segments.Count == 0)
            {
                changeRanges.Free();
                return this;
            }

            if (position < this.Length)
            {
                var subText = this.GetSubText(new TextSpan(position, this.Length - position));
                CompositeText.AddSegments(segments, subText);
            }

            var newText = CompositeText.ToSourceTextAndFree(segments, this, adjustSegments: true);
            if (newText != this)
            {
                return new ChangedText(this, newText, changeRanges.ToImmutableAndFree());
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Constructs a new SourceText from this text with the specified changes.
        /// </summary>
        public SourceText WithChanges(params TextChange[] changes)
        {
            return this.WithChanges((IEnumerable<TextChange>)changes);
        }

        /// <summary>
        /// Returns a new SourceText with the specified span of characters replaced by the new text.
        /// </summary>
        public SourceText Replace(TextSpan span, string newText)
        {
            return this.WithChanges(new TextChange(span, newText));
        }

        /// <summary>
        /// Returns a new SourceText with the specified range of characters replaced by the new text.
        /// </summary>
        public SourceText Replace(int start, int length, string newText)
        {
            return this.Replace(new TextSpan(start, length), newText);
        }

        /// <summary>
        /// Gets the set of <see cref="TextChangeRange"/> that describe how the text changed
        /// between this text an older version. This may be multiple detailed changes
        /// or a single change encompassing the entire text.
        /// </summary>
        public virtual IReadOnlyList<TextChangeRange> GetChangeRanges(SourceText oldText)
        {
            if (oldText == null)
            {
                throw new ArgumentNullException(nameof(oldText));
            }

            if (oldText == this)
            {
                return TextChangeRange.NoChanges;
            }
            else
            {
                return ImmutableArray.Create(new TextChangeRange(new TextSpan(0, oldText.Length), this.Length));
            }
        }

        /// <summary>
        /// Gets the set of <see cref="TextChange"/> that describe how the text changed
        /// between this text and an older version. This may be multiple detailed changes 
        /// or a single change encompassing the entire text.
        /// </summary>
        public virtual IReadOnlyList<TextChange> GetTextChanges(SourceText oldText)
        {
            int newPosDelta = 0;

            var ranges = this.GetChangeRanges(oldText).ToList();
            var textChanges = new List<TextChange>(ranges.Count);

            foreach (var range in ranges)
            {
                var newPos = range.Span.Start + newPosDelta;

                // determine where in the newText this text exists
                string newt;
                if (range.NewLength > 0)
                {
                    var span = new TextSpan(newPos, range.NewLength);
                    newt = this.ToString(span);
                }
                else
                {
                    newt = string.Empty;
                }

                textChanges.Add(new TextChange(range.Span, newt));

                newPosDelta += range.NewLength - range.Span.Length;
            }

            return textChanges.ToImmutableArrayOrEmpty();
        }

        #endregion

        #region Lines

        /// <summary>
        /// The collection of individual text lines.
        /// </summary>
        public TextLineCollection Lines
        {
            get
            {
                var info = _lazyLineInfo;
                return info ?? Interlocked.CompareExchange(ref _lazyLineInfo, info = GetLinesCore(), null) ?? info;
            }
        }

        internal bool TryGetLines(out TextLineCollection lines)
        {
            lines = _lazyLineInfo;
            return lines != null;
        }

        /// <summary>
        /// Called from <see cref="Lines"/> to initialize the <see cref="TextLineCollection"/>. Thereafter,
        /// the collection is cached.
        /// </summary>
        /// <returns>A new <see cref="TextLineCollection"/> representing the individual text lines.</returns>
        protected virtual TextLineCollection GetLinesCore()
        {
            return new LineInfo(this, ParseLineStarts());
        }

        internal sealed class LineInfo : TextLineCollection
        {
            private readonly SourceText _text;
            private readonly int[] _lineStarts;
            private int _lastLineNumber;

            public LineInfo(SourceText text, int[] lineStarts)
            {
                _text = text;
                _lineStarts = lineStarts;
            }

            public override int Count => _lineStarts.Length;

            public override TextLine this[int index]
            {
                get
                {
                    if (index < 0 || index >= _lineStarts.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    int start = _lineStarts[index];
                    if (index == _lineStarts.Length - 1)
                    {
                        return TextLine.FromSpan(_text, TextSpan.FromBounds(start, _text.Length));
                    }
                    else
                    {
                        int end = _lineStarts[index + 1];
                        return TextLine.FromSpan(_text, TextSpan.FromBounds(start, end));
                    }
                }
            }

            public override int IndexOf(int position)
            {
                if (position < 0 || position > _text.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(position));
                }

                int lineNumber;

                // it is common to ask about position on the same line 
                // as before or on the next couple lines
                var lastLineNumber = _lastLineNumber;
                if (position >= _lineStarts[lastLineNumber])
                {
                    var limit = Math.Min(_lineStarts.Length, lastLineNumber + 4);
                    for (int i = lastLineNumber; i < limit; i++)
                    {
                        if (position < _lineStarts[i])
                        {
                            lineNumber = i - 1;
                            _lastLineNumber = lineNumber;
                            return lineNumber;
                        }
                    }
                }

                // Binary search to find the right line
                // if no lines start exactly at position, round to the left
                // EoF position will map to the last line.
                lineNumber = _lineStarts.BinarySearch(position);
                if (lineNumber < 0)
                {
                    lineNumber = (~lineNumber) - 1;
                }

                _lastLineNumber = lineNumber;
                return lineNumber;
            }

            public override TextLine GetLineFromPosition(int position)
            {
                return this[IndexOf(position)];
            }
        }

        private void EnumerateChars(Action<int, char[], int> action)
        {
            var position = 0;
            var buffer = s_charArrayPool.Allocate();

            var length = this.Length;
            while (position < length)
            {
                var contentLength = Math.Min(length - position, buffer.Length);
                this.CopyTo(position, buffer, 0, contentLength);
                action(position, buffer, contentLength);
                position += contentLength;
            }

            // once more with zero length to signal the end
            action(position, buffer, 0);

            s_charArrayPool.Free(buffer);
        }

        private int[] ParseLineStarts()
        {
            // Corner case check
            if (0 == this.Length)
            {
                return new[] { 0 };
            }

            var lineStarts = ArrayBuilder<int>.GetInstance();
            lineStarts.Add(0); // there is always the first line

            var lastWasCR = false;

            // The following loop goes through every character in the text. It is highly
            // performance critical, and thus inlines knowledge about common line breaks
            // and non-line breaks.
            EnumerateChars((int position, char[] buffer, int length) =>
            {
                var index = 0;
                if (lastWasCR)
                {
                    if (length > 0 && buffer[0] == '\n')
                    {
                        index++;
                    }

                    lineStarts.Add(position + index);
                    lastWasCR = false;
                }

                while (index < length)
                {
                    char c = buffer[index];
                    index++;

                    // Common case - ASCII & not a line break
                    // if (c > '\r' && c <= 127)
                    // if (c >= ('\r'+1) && c <= 127)
                    const uint bias = '\r' + 1;
                    if (unchecked(c - bias) <= (127 - bias))
                    {
                        continue;
                    }

                    // Assumes that the only 2-char line break sequence is CR+LF
                    if (c == '\r')
                    {
                        if (index < length && buffer[index] == '\n')
                        {
                            index++;
                        }
                        else if (index >= length)
                        {
                            lastWasCR = true;
                            continue;
                        }
                    }
                    else if (!TextUtilities.IsAnyLineBreakCharacter(c))
                    {
                        continue;
                    }

                    // next line starts at index
                    lineStarts.Add(position + index);
                }
            });

            return lineStarts.ToArrayAndFree();
        }
        #endregion

        /// <summary>
        /// Compares the content with content of another <see cref="SourceText"/>.
        /// </summary>
        public bool ContentEquals(SourceText other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ContentEqualsImpl(other);
        }

        /// <summary>
        /// Implements equality comparison of the content of two different instances of <see cref="SourceText"/>.
        /// </summary>
        protected virtual bool ContentEqualsImpl(SourceText other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.Length != other.Length)
            {
                return false;
            }

            var buffer1 = s_charArrayPool.Allocate();
            var buffer2 = s_charArrayPool.Allocate();
            try
            {
                int position = 0;
                while (position < this.Length)
                {
                    int n = Math.Min(this.Length - position, buffer1.Length);
                    this.CopyTo(position, buffer1, 0, n);
                    other.CopyTo(position, buffer2, 0, n);

                    for (int i = 0; i < n; i++)
                    {
                        if (buffer1[i] != buffer2[i])
                        {
                            return false;
                        }
                    }

                    position += n;
                }

                return true;
            }
            finally
            {
                s_charArrayPool.Free(buffer2);
                s_charArrayPool.Free(buffer1);
            }
        }

        private class StaticContainer : SourceTextContainer
        {
            private readonly SourceText _text;

            public StaticContainer(SourceText text)
            {
                _text = text;
            }

            public override SourceText CurrentText => _text;

            public override event EventHandler<TextChangeEventArgs> TextChanged
            {
                add
                {
                    // do nothing
                }

                remove
                {
                    // do nothing
                }
            }
        }
    }
}