using System.Collections.Generic;
using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTCompound : ASTNode
    {
        public ASTCompound(Token token, IEnumerable<ASTNode> children)
        {
            Token = token;
            Children.AddRange(children);
        }

        public List<ASTNode> Children { get; } = new List<ASTNode>();
    }
}
