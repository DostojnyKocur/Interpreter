using Interpreter.Extensions;
using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTNumber : ASTNode
    {
        public ASTNumber(Token token) => Token = token;

        public double Value => Token.Value.ToNumber();
    }
}
