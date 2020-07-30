using Interpreter.LexerService.Tokens;
using Interpreter.ParserService.AST;

namespace Interpreter.ParserService
{
    public partial class Parser
    {
        private ASTType Type()
        {
            var type = (ASTNode)NonArrayType();

            if (_currentToken.Type == TokenType.LeftBracket)
            {
                type = ArrayType(type as ASTNonArrayType);
            }

            return new ASTType(type.Token, type);
        }

        private ASTNonArrayType NonArrayType()
        {
            var token = _currentToken;
            Eat(token.Type);
            return new ASTNonArrayType(token);
        }

        private ASTArrayType ArrayType(ASTNonArrayType type)
        {
            var rank = new ASTRankSpec(_currentToken);
            Eat(TokenType.LeftBracket);
            Eat(TokenType.RigthBracket);
            return new ASTArrayType(type.Token, type, rank);
        }
    }
}
