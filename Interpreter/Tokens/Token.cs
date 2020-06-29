namespace Interpreter.Tokens
{
    public class Token
    {
        public Token(TokenType type, string value = null) => (Type, Value) = (type, value);

        public TokenType Type { get; }
        public string Value { get; }

        public override string ToString() => $"Token({Type}, {Value})";
    }
}
