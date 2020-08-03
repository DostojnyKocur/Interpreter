using System.Collections.Generic;
using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTFunctionDefinition : ASTNode
    {
        public ASTFunctionDefinition(Token token, ASTType returnType, IEnumerable<ASTParam> parameterList, ASTCompound body)
        {
            ReturnType = returnType;
            Token = token;
            Body = body;
            Parameters.AddRange(parameterList);
        }

        public ASTType ReturnType { get; }
        public List<ASTParam> Parameters { get; } = new List<ASTParam>();
        public ASTCompound Body { get; }
    }
}
