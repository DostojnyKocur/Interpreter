using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTWhile : ASTNode
    {
        public ASTWhile(Token token, ASTNode condition, ASTNode body) => (Token, Condition, Body) = (token, condition, body);

        public ASTNode Condition { get; }
        public ASTNode Body { get; }
    }
}
