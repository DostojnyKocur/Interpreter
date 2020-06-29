namespace Interpreter.Tokens
{
    public enum TokenType
    {
        TypeNumber,
        ConstNumber,

        Plus,
        Minus,
        Multiplication,
        Divide,
        Modulo,

        Id,

        ScopeBegin,
        ScopeEnd,
        LeftParen,
        RightParen,
        Assign,
        Semicolon,
        Comma,

        EOF
    }
}
