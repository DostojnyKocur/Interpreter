using Interpreter.LexerService.Tokens;

namespace Interpreter.AST
{
    public class ASTVariable : ASTNode
    {
        public ASTVariable(Token token) => (Token) = (token);

        public string Name => Token.Value;
    }
}
