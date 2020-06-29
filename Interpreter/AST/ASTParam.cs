namespace Interpreter.AST
{
    public class ASTParam : ASTNode
    {
        public ASTParam(ASTType type, ASTVariable variable) => (Type, Variable) = (type, variable);
        
        public ASTType Type { get; }

        public ASTVariable Variable { get; }
    }
}
