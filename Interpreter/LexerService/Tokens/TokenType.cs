using System.Collections.Generic;

namespace Interpreter.LexerService.Tokens
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
        Else,
        While,

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

        Id,

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
        Comma,

        EOF
    }

    public static class TokenTypes
    {
        public static readonly Dictionary<char, TokenType> For = new Dictionary<char, TokenType>
        {
            { '+', TokenType.Plus },
            { '-', TokenType.Minus },
            { '*', TokenType.Multiplication },
            { '%', TokenType.Modulo },
            { '(', TokenType.LeftParen },
            { ')', TokenType.RightParen },
            { '{', TokenType.ScopeBegin },
            { '}', TokenType.ScopeEnd },
            { '[', TokenType.LeftBracket },
            { ']', TokenType.RigthBracket },
            { '=', TokenType.Assign },
            { ';', TokenType.Semicolon },
            { ',', TokenType.Comma }
        };

        public static TokenType? GetForChar(char @char)
        {
            if (For.ContainsKey(@char))
            {
                return For[@char];
            }
            else
            {
                return null;
            }
        }
    }
}
