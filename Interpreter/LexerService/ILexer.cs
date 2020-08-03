using Interpreter.Common.Tokens;

namespace Interpreter.LexerService
{
    public interface ILexer
    {
        char CurrentChar { get; }

        Token GetNextToken();
    }
}
