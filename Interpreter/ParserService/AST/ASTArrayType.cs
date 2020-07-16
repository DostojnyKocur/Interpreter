using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    class ASTArrayType : ASTNode
    {
        public ASTArrayType(Token token, ASTNonArrayType arrayType, ASTRankSpec rankType)
            => (Token, ArrayType, RankType) = (token, arrayType, rankType);

        public ASTNonArrayType ArrayType { get; }
        public ASTRankSpec RankType { get; }
        public string Name => Token.Value;
    }
}
