using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTVariable : ASTNode
    {
        public ASTVariable(Token token) => Token = token;

        public string Name => Token.Value;
    }
}
