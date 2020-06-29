using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTFunctionDefinition : ASTNode
    {
        public ASTFunctionDefinition(ASTType returnType, Token name, ASTArgumentList argumentList, ASTCompound body) 
            => (ReturnType, Name, Arguments, Body) = (returnType, name, argumentList, body);

        public Token Name { get; }
        public ASTType ReturnType { get; }
        public ASTArgumentList Arguments { get; }
        public ASTCompound Body { get; }
    }
}
