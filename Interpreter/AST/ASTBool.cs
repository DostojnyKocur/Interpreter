using Interpreter.Extensions;
using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTBool : ASTNode
    {
        private readonly Token _token;

        public ASTBool(Token token) => (_token) = (token);

        public bool Value => _token.Value.ToBool();
    }
}
