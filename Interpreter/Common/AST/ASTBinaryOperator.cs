using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTBinaryOperator : ASTNode
    {
        public ASTBinaryOperator(ASTNode left, Token operation, ASTNode right) => (Left, Token, Right) = (left, operation, right);

        public ASTNode Left { get; }
        public ASTNode Right { get; }
    }
}
