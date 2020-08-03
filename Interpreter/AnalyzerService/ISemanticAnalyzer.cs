using Interpreter.Common.AST;

namespace Interpreter.AnalyzerService
{
    public interface ISemanticAnalyzer
    {
        void Analyze(ASTNode node);
    }
}
