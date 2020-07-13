using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTContinue : ASTNode
    {
        public ASTContinue(Token token) => (Token) = (token);
        public Token Token { get; }
    }
}
