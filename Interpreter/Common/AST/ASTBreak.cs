using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTBreak : ASTNode
    {
        public ASTBreak(Token token) => Token = token;
    }
}
