namespace Interpreter.AST
{
    public class ASTProgram : ASTNode
    {
        public ASTProgram(ASTNode root) => (Root) = (root);

        public ASTNode Root { get; }
    }
}
