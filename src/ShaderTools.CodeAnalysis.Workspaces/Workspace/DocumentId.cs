using System.Collections.Generic;
using System.Diagnostics;

namespace ShaderTools.CodeAnalysis
{
    /// <summary>
    /// An identifier that can be used to retrieve the same <see cref="Document"/> across versions of the
    /// workspace.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    public sealed class DocumentId
    {
        /// <summary>
        /// Canonical form of the document file path.
        /// </summary>
        public string Id { get; }

        internal string OriginalFilePath { get; }

        public DocumentId(string filePath)
        {
            Id = filePath.ToLowerInvariant();
            OriginalFilePath = filePath;
        }

        internal string GetDebuggerDisplay()
        {
            return string.Format("({0}, #{1} - {2})", GetType().Name, Id, OriginalFilePath);
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
