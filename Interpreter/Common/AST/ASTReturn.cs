using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTReturn : ASTNode
    {
        public ASTReturn(Token token, ASTNode expression) => (Token, Expression) = (token, expression);

        public ASTNode Expression { get; }
    }
}
