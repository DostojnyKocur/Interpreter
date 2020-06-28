using System.Collections.Generic;

namespace Interpreter.AST
{
    public class ASTArgumentList : ASTNode
    {
        public ASTArgumentList(IEnumerable<ASTVariableDeclaration> children)
        {
            Arguments.AddRange(children);
        }

        public List<ASTVariableDeclaration> Arguments { get; } = new List<ASTVariableDeclaration>();
    }
}
