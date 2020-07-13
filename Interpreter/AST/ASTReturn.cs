using Interpreter.LexerService.Tokens;

namespace Interpreter.AST
{
    public class ASTReturn : ASTNode
    {
        public ASTReturn(Token token, ASTNode condition) => (Token, Condition) = (token, condition);

        public ASTNode Condition { get; }
    }
}
