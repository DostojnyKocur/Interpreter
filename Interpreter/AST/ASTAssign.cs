using Interpreter.LexerService.Tokens;

namespace Interpreter.AST
{
    public class ASTAssign : ASTNode
    {
        public ASTAssign(ASTNode left, Token operation, ASTNode right) => (Left, Token, Right) = (left, operation, right);

        public ASTNode Left { get; }
        public ASTNode Right { get; }
    }
}
