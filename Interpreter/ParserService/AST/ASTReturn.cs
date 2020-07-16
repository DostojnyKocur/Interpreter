using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTReturn : ASTNode
    {
        public ASTReturn(Token token, ASTNode condition) => (Token, Condition) = (token, condition);

        public ASTNode Condition { get; }
    }
}
