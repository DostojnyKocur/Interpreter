using Interpreter.LexerService.Tokens;

namespace Interpreter.AST
{
    public abstract class ASTNode
    {
        public Token Token { get; protected set; }
        public TokenType Type => Token.Type;
    }
}
