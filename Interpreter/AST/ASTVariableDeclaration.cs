namespace Interpreter.AST
{
    public class ASTVariableDeclaration : ASTNode
    {
        public ASTVariableDeclaration(ASTVariable variable, ASTType type) => (Variable, Type) = (variable, type);

        public ASTVariable Variable { get; }
        public ASTType Type { get; }
    }
}
