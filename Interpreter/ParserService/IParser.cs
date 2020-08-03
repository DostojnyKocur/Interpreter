using Interpreter.Common.AST;

namespace Interpreter.ParserService
{
    public interface IParser
    {
        ASTNode Parse();
    }
}
