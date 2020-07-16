using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTContinue : ASTNode
    {
        public ASTContinue(Token token) => Token = token;
    }
}
