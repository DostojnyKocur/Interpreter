using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTElif : ASTNode
    {
        public ASTElif(Token token, ASTNode condition, ASTNode ifTrue) => (Token, Condition, IfTrue) = (token, condition, ifTrue);

        public ASTNode Condition { get; }
        public ASTNode IfTrue { get; }
    }
}
