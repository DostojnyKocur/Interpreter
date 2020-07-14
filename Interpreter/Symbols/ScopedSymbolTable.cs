using System;
using System.Collections.Generic;

namespace Interpreter.Symbols
{
    public class ScopedSymbolTable
    {
        private readonly Dictionary<string, Symbol> _symbols = new Dictionary<string, Symbol>
        {
            { "void", new SymbolBuiltinType("void") },
            { "number", new SymbolBuiltinType("number") },
            { "bool", new SymbolBuiltinType("bool") },
            { "string", new SymbolBuiltinType("string") }
        };

        public ScopedSymbolTable(string scopeName, uint scopeLevel, ScopedSymbolTable enclosingScope = null)
        {
            (Name, Level, EnclosingScope) = (scopeName, scopeLevel, enclosingScope);

            var printFunction = new SymbolBuiltinFunction("print", _symbols["void"]);
            printFunction.Parameters.Add(new SymbolVariable("str", _symbols["string"]));
            _symbols.Add("print", printFunction);
        }

        public string Name { get; }

        public uint Level { get; }

        public ScopedSymbolTable EnclosingScope { get; }

        public void DebugPrintSymbols()
        {
            Logger.DebugScope(Environment.NewLine);
            var enclosingScopeName = EnclosingScope != null ? EnclosingScope.Name : "none";
            Logger.DebugScope($"{Environment.NewLine}==== SYMBOL TABLE ({Name} : level {Level} : enclosing scope : {enclosingScopeName}) ====");
            foreach (var entry in _symbols)
            {
                Logger.DebugScope(string.Format("{0, 20}\t:\t{1, -30}", entry.Key.Trim(), entry.Value));
            }
            Logger.DebugScope("==== ==== ====");
        }

        public void Define(Symbol symbol)
        {
            Logger.DebugScope($"Define symbol: {symbol}");

            symbol.ScopeLevel = Level;
            _symbols.Add(symbol.Name, symbol);
        }

        public Symbol Lookup(string symbolName, bool onlyCurrentScope = false)
        {
            Logger.DebugScope($"Lookup symbol: {symbolName} (scope {Name})");
            if (_symbols.ContainsKey(symbolName))
            {
                return _symbols[symbolName];
            }
            else if (EnclosingScope != null && !onlyCurrentScope)
            {
                return EnclosingScope.Lookup(symbolName);
            }

            return null;
        }
    }
}
