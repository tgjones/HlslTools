using Microsoft.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Parser
{
    internal sealed class CharReader
    {
        private readonly SourceText _text;

        public CharReader(SourceText text)
        {
            _text = text;
        }

        public void NextChar() => Position++;

        public int Position { get; private set; }

        public char Current => Peek(0);

        public char Peek() => Peek(1);

        public char Peek(int offset)
        {
            var index = Position + offset;
            return (index < _text.Length)
                ? _text[index]
                : '\0';
        }

        public void Reset(int position)
        {
            Position = position;
        }
    }
}