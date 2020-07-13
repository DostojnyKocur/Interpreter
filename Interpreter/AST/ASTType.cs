using Interpreter.LexerService.Tokens;

namespace Interpreter.AST
{
    public class ASTType : ASTNode
    {
        public ASTType(Token token) => (Token) = (token);

        public string Name => Token.Value;
    }
}
