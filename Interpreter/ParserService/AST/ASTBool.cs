﻿using Interpreter.Common.Extensions;
using Interpreter.LexerService.Tokens;

namespace Interpreter.ParserService.AST
{
    public class ASTBool : ASTNode
    {
        public ASTBool(Token token) => Token = token;

        public bool Value => Token.Value.ToBool();
    }
}
