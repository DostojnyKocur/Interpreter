using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public abstract class ASTNode
    {
        public Token Token { get; protected set; }
        public TokenType Type => Token.Type;
    }
}
