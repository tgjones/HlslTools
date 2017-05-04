using System;

namespace ShaderTools.CodeAnalysis.Text
{
    public sealed class SourceFile
    {
        public SourceText Text { get; }

        public string FilePath => Text.FilePath;

        /// <summary>
        /// The <see cref="SourceFile"/> that #include'd this <see cref="SourceFile"/>. 
        /// Returns <code>null</code> for the main / root file.
        /// </summary>
        public SourceFile IncludedBy { get; }

        public bool IsRootFile => IncludedBy == null;

        internal SourceFile(SourceText text, SourceFile includedBy)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IncludedBy = includedBy;
        }
    }
}