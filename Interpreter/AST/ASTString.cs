using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTString : ASTNode
    {
        private readonly Token _token;

        public ASTString(Token token) => (_token) = (token);

        public string Value => _token.Value;
    }
}
