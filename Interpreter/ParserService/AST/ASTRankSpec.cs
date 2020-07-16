using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTRankSpec : ASTNode
    {
        public ASTRankSpec(Token token) => Token = token;

        public string Name => Token.Value;
    }
}
