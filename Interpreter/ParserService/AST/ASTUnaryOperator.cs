﻿using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTUnaryOperator : ASTNode
    {
        public ASTUnaryOperator(Token operation, ASTNode expression) => (Token, Expression) = (operation, expression);

        public ASTNode Expression { get; }
    }

}
