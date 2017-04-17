using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ShaderTools.EditorServices.Workspace
{
    /// <summary>
    /// An identifier that can be used to retrieve the same <see cref="Document"/> across versions of the
    /// workspace.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    public sealed class DocumentId
    {
        public Guid Id { get; }

        private readonly string _debugName;

        internal string DebugName => _debugName;

        private DocumentId(Guid guid, string debugName)
        {
            Id = guid;
            _debugName = debugName;
        }

        /// <summary>
        /// Creates a new <see cref="DocumentId"/> instance.
        /// </summary>
        /// <param name="debugName">An optional name to make this id easier to recognize while debugging.</param>
        public static DocumentId CreateNewId(string debugName = null)
        {
            return new DocumentId(Guid.NewGuid(), debugName);
        }

        internal string GetDebuggerDisplay()
        {
            return string.Format("({0}, #{1} - {2})", GetType().Name, Id, _debugName);
        }

        public override string ToString()
        {
            return GetDebuggerDisplay();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DocumentId);
        }

        public bool Equals(DocumentId other)
        {
            return
                !ReferenceEquals(other, null) &&
                Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(DocumentId left, DocumentId right)
        {
            return EqualityComparer<DocumentId>.Default.Equals(left, right);
        }

        public static bool operator !=(DocumentId left, DocumentId right)
        {
            return !(left == right);
        }
    }
}
