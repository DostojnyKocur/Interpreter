namespace Interpreter.AST
{
    public class ASTAssign : ASTNode
    {
        private readonly Token _operation;

        public ASTAssign(ASTVariable left, Token operation, ASTNode right) => (Left, _operation, Right) = (left, operation, right);

        public TokenType Type => _operation.Type;
        public ASTVariable Left { get; }
        public ASTNode Right { get; }
    }
}
