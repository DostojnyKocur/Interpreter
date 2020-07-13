using System;
using Interpreter.LexerService.Tokens;

namespace Interpreter.Errors
{
    public abstract class CustomError : Exception
    {
        public CustomError()
            : base()
        {

        }

        public CustomError(ErrorCode? errorCode = null, Token token = null, string message = null)
            : base(message)
        {
            ErrorCode = errorCode;
            Token = token;
        }

        public ErrorCode? ErrorCode { get; }

        public Token Token { get; }

        public override string ToString()
        {
            return $"{GetType()}: {Message}";
        }
    }

    public class LexerError : CustomError
    {
        public LexerError(ErrorCode? errorCode = null, Token token = null, string message = null)
            : base(errorCode, token, message)
        {

        }
    }

    public class ParserError : CustomError
    {
        public ParserError(ErrorCode? errorCode = null, Token token = null, string message = null)
            : base(errorCode, token, message)
        {

        }
    }

    public class SemanticError : CustomError
    {
        public SemanticError(ErrorCode? errorCode = null, Token token = null, string message = null)
            : base(errorCode, token, message)
        {

        }
    }
}
