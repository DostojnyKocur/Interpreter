﻿using System.Collections.Generic;
using Interpreter.AST;

namespace Interpreter.Symbols
{
    public class SymbolFunction : Symbol
    {
        public SymbolFunction(string name, Symbol returnType)
            : base(name)
        {
            ReturnType = returnType;
        }

        public Symbol ReturnType { get; }
        public List<SymbolVariable> Parameters { get; } = new List<SymbolVariable>();
        public ASTCompound Body { get; set; }

        public override string ToString()
        {
            var paramList = new List<string>();
            Parameters.ForEach(param => paramList.Add($"{param.Name}:{param.Type}"));
            var stringParamList = string.Join(", ", paramList);


            return $"<{Name}({stringParamList}):{ReturnType.Name}>";
        }
    }
}
