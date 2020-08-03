using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    class ASTNonArrayType : ASTNode
    {
        public ASTNonArrayType(Token token) => Token = token;

        public string Name => Token.Value;
    }
}
