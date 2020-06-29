using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTVariable : ASTNode
    {
        public ASTVariable(Token token) => (Token) = (token);

        public Token Token { get; }

        public string Name => Token.Value;
    }
}
