namespace Interpreter
{
    public enum TokenType
    {
        Number,

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

        EOF
    }
}
