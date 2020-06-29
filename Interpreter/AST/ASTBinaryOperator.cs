using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTBinaryOperator : ASTNode
    {
        private readonly Token _operation;

        public ASTBinaryOperator(ASTNode left, Token operation, ASTNode right) => (Left, _operation, Right) = (left, operation, right);

        public TokenType Type => _operation.Type;
        public ASTNode Left { get; }
        public ASTNode Right { get; }
    }
}
