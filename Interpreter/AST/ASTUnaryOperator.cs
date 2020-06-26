namespace Interpreter.AST
{
    public class ASTUnaryOperator : ASTNode
    {
        private readonly Token _operation;

        public ASTUnaryOperator(Token operation, ASTNode expression) => (_operation, Expression) = (operation, expression);

        public TokenType Type => _operation.Type;
        public ASTNode Expression { get; }
    }

}
