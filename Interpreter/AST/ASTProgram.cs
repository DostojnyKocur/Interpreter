using Interpreter.Symbols;
using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTProgram : ASTNode
    {
        public ASTProgram(ASTNode root, Token token) => (Root, Token) = (root, token);

        public ASTNode Root { get; }
        public Token Token { get; }

        public SymbolFunction MainFunction { get; set; }
    }
}
