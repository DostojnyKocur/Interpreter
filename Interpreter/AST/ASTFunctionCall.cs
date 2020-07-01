using System.Collections.Generic;
using Interpreter.Tokens;

namespace Interpreter.AST
{
    public class ASTFunctionCall : ASTNode
    {
        public ASTFunctionCall(string functionName, IEnumerable<ASTNode> actualParameters, Token token)
        {
            FunctionName = functionName;
            Token = token;
            ActualParameters.AddRange(actualParameters);
        }

        public string FunctionName { get; }
        public List<ASTNode> ActualParameters { get; } = new List<ASTNode>();
        public Token Token { get; }
    }
}
