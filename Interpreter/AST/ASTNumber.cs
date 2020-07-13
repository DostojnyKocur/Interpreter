using Interpreter.Extensions;
using Interpreter.LexerService.Tokens;

namespace Interpreter.AST
{
    public class ASTNumber : ASTNode
    {
        public ASTNumber(Token token) => (Token) = (token);

        public double Value => Token.Value.ToNumber(); 
    }
}
