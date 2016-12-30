namespace ShaderTools.Core.Text
{
    public struct FilePoint
    {
        public SourceText SourceText { get; }
        public int Position { get; }

        public FilePoint(SourceText sourceText, int position)
        {
            SourceText = sourceText;
            Position = position;
        }
    }
}
