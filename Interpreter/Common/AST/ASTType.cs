using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTType : ASTNode
    {
        public ASTType(Token token, ASTNode type) => (Token, TypeSpec) = (token, type);

        public ASTNode TypeSpec { get; }
        public string Name => Token.Value;
    }
}
