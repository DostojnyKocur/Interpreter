namespace Interpreter.LexerService.Tokens
{
    public class Token
    {
        public Token(TokenType type, string value = null, uint? lineNumber = null, uint? column = null) 
            => (Type, Value, LineNumber, Column) = (type, value, lineNumber, column);

        public TokenType Type { get; }
        public string Value { get; }
        public uint? LineNumber { get; }
        public uint? Column { get; }

        public override string ToString() => $"Token({Type}, {Value}, position={LineNumber}:{Column})";

        public Token Copy(uint lineNumber, uint column)
        {
            return new Token(Type, Value, lineNumber, column);
        }
    }
}
