using Interpreter.LexerService.Tokens;

namespace Interpreter.AST
{
    class ASTNonArrayType : ASTNode
    {
        public ASTNonArrayType(Token token) => (Token) = (token);

        public string Name => Token.Value;
    }
}
