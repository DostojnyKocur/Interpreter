using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public abstract class ASTNode
    {
        public Token Token { get; protected set; }
        public TokenType Type => Token.Type;
    }
}
