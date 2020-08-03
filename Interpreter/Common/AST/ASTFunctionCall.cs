using System.Collections.Generic;
using Interpreter.Common.Symbols;
using Interpreter.Common.Tokens;

namespace Interpreter.Common.AST
{
    public class ASTFunctionCall : ASTNode
    {
        public ASTFunctionCall(Token token, string functionName, IEnumerable<ASTNode> actualParameters)
        {
            FunctionName = functionName;
            Token = token;
            ActualParameters.AddRange(actualParameters);
        }

        public string FunctionName { get; }
        public List<ASTNode> ActualParameters { get; } = new List<ASTNode>();
        public SymbolFunction SymbolFunction { get; set; }
    }
}
