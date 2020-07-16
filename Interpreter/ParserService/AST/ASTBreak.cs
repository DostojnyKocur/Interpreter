using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTBreak : ASTNode
    {
        public ASTBreak(Token token) => Token = token;
    }
}
