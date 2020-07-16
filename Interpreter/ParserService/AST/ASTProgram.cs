using Interpreter.LexerService.Tokens;
using Interpreter.Symbols;

namespace Interpreter.ParserService.AST
{
    public class ASTProgram : ASTNode
    {
        public ASTProgram(Token token, ASTNode root) => (Token, Root) = (token, root);

        public ASTNode Root { get; }
        public SymbolFunction MainFunction { get; set; }
    }
}
