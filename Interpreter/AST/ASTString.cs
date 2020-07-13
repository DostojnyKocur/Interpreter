using Interpreter.LexerService.Tokens;

namespace Interpreter.AST
{
    public class ASTString : ASTNode
    {
        public ASTString(Token token) => (Token) = (token);

        public string Value => Token.Value;
    }
}
