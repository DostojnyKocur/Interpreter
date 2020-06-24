namespace Interpreter.AST
{
    public class ASTUnaryOperator : ASTNode
    {
        private readonly Token _operation;

        public ASTUnaryOperator(Token operation, ASTNode node) => (_operation, Node) = (operation, node);

        public TokenType Type => _operation.Type;
        public ASTNode Node { get; }
    }

}
