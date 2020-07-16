using System.Collections.Generic;
using Interpreter.LexerService.Tokens;

namespace Interpreter.AST
{
    public class ASTArrayInitialization : ASTNode
    {
        public ASTArrayInitialization(Token token, IEnumerable<ASTNode> children)
        {
            Token = token;
            Children.AddRange(children);
        }

        public List<ASTNode> Children { get; } = new List<ASTNode>();
    }
}
