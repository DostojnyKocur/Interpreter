using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTUnaryOperator : ASTNode
    {
        public ASTUnaryOperator(Token operation, ASTNode expression) => (Token, Expression) = (operation, expression);

        public ASTNode Expression { get; }
    }

}
