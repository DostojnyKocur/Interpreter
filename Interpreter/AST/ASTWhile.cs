using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTWhile : ASTNode
    {
        public ASTWhile(Token token, ASTNode condition, ASTNode body) => (Token, Condition, Body) = (token, condition, body);

        public Token Token { get; }
        public ASTNode Condition { get; }
        public ASTNode Body { get; }
    }
}
