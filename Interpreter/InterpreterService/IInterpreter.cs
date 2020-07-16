using Interpreter.ParserService.AST;

namespace Interpreter.InterpreterService
{
    public interface IInterpreter
    {
        void Interpret(ASTNode tree);
    }
}
