using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    class ASTNonArrayType : ASTNode
    {
        public ASTNonArrayType(Token token) => Token = token;

        public string Name => Token.Value;
    }
}
