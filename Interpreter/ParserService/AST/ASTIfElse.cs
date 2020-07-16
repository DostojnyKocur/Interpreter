using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTIfElse : ASTNode
    {
        public ASTIfElse(Token token, ASTNode condition, ASTNode ifTrue, ASTNode @else)
            => (Token, Condition, IfTrue, Else) = (token, condition, ifTrue, @else);

        public ASTNode Condition { get; }
        public ASTNode IfTrue { get; }
        public ASTNode Else { get; }
    }
}
