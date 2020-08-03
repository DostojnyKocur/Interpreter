using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTString : ASTNode
    {
        public ASTString(Token token) => Token = token;

        public string Value => Token.Value;
    }
}
