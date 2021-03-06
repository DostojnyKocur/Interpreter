﻿using System.Collections.Generic;

namespace Interpreter.Common.AST
{
    public class ASTVariablesDeclarations : ASTNode
    {
        public ASTVariablesDeclarations(IEnumerable<ASTVariableDeclaration> children)
        {
            Children.AddRange(children);
        }

        public List<ASTVariableDeclaration> Children { get; } = new List<ASTVariableDeclaration>();
    }
}
