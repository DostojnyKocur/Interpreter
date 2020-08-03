using Interpreter.Common.Symbols;
using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTProgram : ASTNode
    {
        public ASTProgram(Token token, ASTNode root) => (Token, Root) = (token, root);

        public ASTNode Root { get; }
        public SymbolFunction MainFunction { get; set; }
    }
}
