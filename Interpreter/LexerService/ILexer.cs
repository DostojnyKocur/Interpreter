using Interpreter.LexerService.Tokens;

namespace Interpreter.LexerService
{
    public interface ILexer
    {
        char CurrentChar { get; }

        Token GetNextToken();
    }
}
