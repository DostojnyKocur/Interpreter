using Interpreter.ParserService.AST;

namespace Interpreter.ParserService
{
    public interface IParser
    {
        ASTNode Parse();
    }
}
