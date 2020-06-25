using System.Collections.Generic;

namespace Interpreter.AST
{
    public class ASTCompound : ASTNode
    {
        public ASTCompound(IEnumerable<ASTNode> children) 
        {
            Children.AddRange(children);
        }

        public List<ASTNode> Children { get; } = new List<ASTNode>();
    }
}
