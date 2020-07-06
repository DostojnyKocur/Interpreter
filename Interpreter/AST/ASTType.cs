using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTType
    {
        public ASTType(Token token) => (Token) = (token);

        public Token Token { get; }
        public string Name => Token.Value;
    }
}
