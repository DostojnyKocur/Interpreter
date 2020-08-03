using Interpreter.Common.AST;

namespace Interpreter.InterpreterService
{
    public interface IInterpreter
    {
        void Interpret(ASTNode tree);
    }
}
