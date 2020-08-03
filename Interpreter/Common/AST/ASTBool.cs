using Interpreter.Common.Extensions;
using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTBool : ASTNode
    {
        public ASTBool(Token token) => Token = token;

        public bool Value => Token.Value.ToBool();
    }
}
