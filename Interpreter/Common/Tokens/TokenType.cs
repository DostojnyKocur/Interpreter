namespace Interpreter.Common.Tokens
{
    public enum TokenType
    {
        TypeVoid,
        TypeNumber,
        TypeBool,
        TypeString,
        ConstNumber,
        ConstBool,
        ConstString,

        If,
        Elif,
        Else,
        While,
        For,

        Plus,
        Minus,
        Multiplication,
        Divide,
        Modulo,

        Not,
        And,
        Or,
        Equal,
        NotEqual,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,

        Identifier,

        Return,
        Break,
        Continue,

        ScopeBegin,
        ScopeEnd,
        LeftParen,
        RightParen,
        LeftBracket,
        RigthBracket,
        Assign,
        Semicolon,
        Colon,
        Comma,

        EOF
    }
}
