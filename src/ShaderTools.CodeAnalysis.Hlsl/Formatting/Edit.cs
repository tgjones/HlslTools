namespace ShaderTools.CodeAnalysis.Hlsl.Formatting
{
    public sealed class Edit
    {
        public readonly int Start, Length;
        public readonly string Text;

        public Edit(int start, int length, string text)
        {
            Start = start;
            Length = length;
            Text = text;
        }

        public override string ToString()
        {
            return $"Start={Start}, Length={Length}, Text='{Text}'";
        }
    }
}