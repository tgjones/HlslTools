using System;

namespace ShaderTools.CodeAnalysis.Text
{
    /// <summary>
    /// An object that contains an instance of an SourceText and raises events when its current instance
    /// changes.
    /// </summary>
    public abstract class SourceTextContainer
    {
        /// <summary>
        /// The current text instance.
        /// </summary>
        public abstract SourceText CurrentText { get; }

        /// <summary>
        /// Raised when the current text instance changes.
        /// </summary>
        public abstract event EventHandler<TextChangeEventArgs> TextChanged;
    }
}
