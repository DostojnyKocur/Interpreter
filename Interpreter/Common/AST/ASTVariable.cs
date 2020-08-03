using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTVariable : ASTNode
    {
        public ASTVariable(Token token, ASTNode arrayIndexFrom = null, ASTNode arrayIndexTo = null, ASTNode arrayIndexStep = null)
            => (Token, ArrayIndexFrom, ArrayIndexTo, ArrayIndexStep) = (token, arrayIndexFrom, arrayIndexTo, arrayIndexStep);

        public ASTNode ArrayIndexFrom { get; }
        public ASTNode ArrayIndexTo { get; }
        public ASTNode ArrayIndexStep { get; }
        public string Name => Token.Value;
    }
}
