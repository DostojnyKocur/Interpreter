using Interpreter.LexerService.Tokens;

namespace Interpreter.AST
{
    public class ASTType : ASTNode
    {
        public ASTType(Token token, ASTNode type) => (Token, TypeSpec) = (token, type);

        public ASTNode TypeSpec { get; }
        public string Name => Token.Value;
    }
}
