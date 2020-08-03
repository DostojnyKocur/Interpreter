using System.Collections.Generic;
using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTIfElse : ASTNode
    {
        public ASTIfElse(Token token, ASTNode condition, ASTNode ifTrue, IEnumerable<ASTElif> elifs, ASTNode @else)
        {
            Token = token;
            Condition = condition;
            IfTrue = ifTrue;
            Elifs.AddRange(elifs);
            Else = @else;
        }

        public ASTNode Condition { get; }
        public ASTNode IfTrue { get; }
        public List<ASTElif> Elifs { get; } = new List<ASTElif>();
        public ASTNode Else { get; }
    }
}
