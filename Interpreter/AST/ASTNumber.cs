using Interpreter.Extensions;
using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTNumber : ASTNode
    {
        private readonly Token _token;

        public ASTNumber(Token token) => (_token) = (token);

        public double Value => _token.Value.ToNumber(); 
    }
}
