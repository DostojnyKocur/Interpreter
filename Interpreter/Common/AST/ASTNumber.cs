using Interpreter.Common.Extensions;
using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTNumber : ASTNode
    {
        public ASTNumber(Token token) => Token = token;

        public double Value => Token.Value.ToNumber();
    }
}
