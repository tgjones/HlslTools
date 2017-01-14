namespace ShaderTools.Core.Text
{
    public abstract class SourceText
    {
        public static SourceText From(string text, string filename = null)
        {
            return new StringText(text, filename);
        }

        public abstract string GetText(TextSpan textSpan);

        public string GetText(int position, int length)
        {
            return GetText(new TextSpan(this, position, length));
        }

        public string GetText()
        {
            return GetText(0, Length);
        }

        public abstract char this[int index] { get; }

        public abstract int Length { get; }

        public abstract string Filename { get; }

        public abstract int GetLineNumberFromPosition(int position);

        public abstract bool IsRoot { get; }
    }
}