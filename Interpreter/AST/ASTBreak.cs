using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTBreak : ASTNode
    {
        public ASTBreak(Token token) => (Token) = (token);
        public Token Token { get; }
    }
}
