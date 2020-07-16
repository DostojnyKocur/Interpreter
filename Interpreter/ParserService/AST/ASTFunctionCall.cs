using System.Collections.Generic;
using Interpreter.AnalyzerService.Symbols;
using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
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
