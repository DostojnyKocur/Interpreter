namespace Interpreter.Common.AST
{
    public class ASTVariableDeclaration : ASTNode
    {
        public ASTVariableDeclaration(ASTType type, ASTVariable variable) => (Token, VariableType, Variable) = (type.Token, type, variable);

        public ASTVariable Variable { get; }
        public ASTType VariableType { get; }
    }
}
