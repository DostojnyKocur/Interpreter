using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTReturn : ASTNode
    {
        public ASTReturn(Token token, ASTNode expression) => (Token, Expression) = (token, expression);

        public Token Token { get; }
        public ASTNode Expression { get; }
    }
}
