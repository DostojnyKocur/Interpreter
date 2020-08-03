using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTContinue : ASTNode
    {
        public ASTContinue(Token token) => Token = token;
    }
}
