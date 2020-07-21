using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTVariable : ASTNode
    {
        public ASTVariable(Token token, ASTNode arrayIndex = null) => (Token, ArrayIndex) = (token, arrayIndex);

        public ASTNode ArrayIndex { get; }
        public string Name => Token.Value;
    }
}
