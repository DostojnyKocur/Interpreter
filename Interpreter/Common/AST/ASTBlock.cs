using System.Collections.Generic;
using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTBlock : ASTNode
    {
        public ASTBlock(Token token, IEnumerable<ASTNode> children)
        {
            Token = token;
            Children.AddRange(children);
        }

        public List<ASTNode> Children { get; } = new List<ASTNode>();
    }
}
