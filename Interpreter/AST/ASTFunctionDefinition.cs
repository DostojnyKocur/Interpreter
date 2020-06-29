using System.Collections.Generic;
using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTFunctionDefinition : ASTNode
    {
        public ASTFunctionDefinition(ASTType returnType, Token name, List<ASTParam> parameterList, ASTCompound body)
        {
            ReturnType = returnType;
            Name = name;
            Body = body;
            Parameters.AddRange(parameterList);
        }

        public Token Name { get; }
        public ASTType ReturnType { get; }
        public List<ASTParam> Parameters { get; } = new List<ASTParam>();
        public ASTCompound Body { get; }
    }
}
