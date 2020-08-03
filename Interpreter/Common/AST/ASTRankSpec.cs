using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTRankSpec : ASTNode
    {
        public ASTRankSpec(Token token) => Token = token;

        public string Name => Token.Value;
    }
}
