using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTAssign : ASTNode
    {
        private readonly Token _operation;

        public ASTAssign(ASTNode left, Token operation, ASTNode right) => (Left, _operation, Right) = (left, operation, right);

        public TokenType Type => _operation.Type;
        public ASTNode Left { get; }
        public ASTNode Right { get; }
    }
}
