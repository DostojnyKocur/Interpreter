namespace Interpreter.AST
{
    public class ASTParam : ASTNode
    {
        public ASTParam(ASTType type, ASTVariable variable) => (Token, VariableType, Variable) = (type.Token, type, variable);
        
        public ASTType VariableType { get; }
        public ASTVariable Variable { get; }
    }
}
