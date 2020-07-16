using Interpreter.LexerService.Tokens;

namespace Interpreter.AST
{
    public class ASTIndexExpression : ASTNode
    {
        public ASTIndexExpression(Token token, ASTVariable variable, ASTNode expression) 
            => (Token, Variable, Expression) = (token, variable, expression);

        public ASTVariable Variable { get; }
        public ASTNode Expression { get; }
    }
}
