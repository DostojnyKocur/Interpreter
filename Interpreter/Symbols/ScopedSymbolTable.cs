using System;
using System.Collections.Generic;

namespace Interpreter.Symbols
{
    public class ScopedSymbolTable
    {
        private readonly Dictionary<string, Symbol> _symbols = new Dictionary<string, Symbol>
        {
            { "number", new Symbol("number") }
        };

        public ScopedSymbolTable(string scopeName, uint scopeLevel, ScopedSymbolTable enclosingScope = null) 
            => (Name, Level, EnclosingScope) = (scopeName, scopeLevel, enclosingScope);

        public string Name { get; }

        public uint Level { get; }

        public ScopedSymbolTable EnclosingScope { get; }

        public void DebugPrintSymbols()
        {
            var enclosingScopeName = EnclosingScope != null ? EnclosingScope.Name : "none";
            Console.WriteLine($"\r\n==== SYMBOL TABLE ({Name} : level {Level} : enclosing scope : {enclosingScopeName}) ====");
            foreach (var entry in _symbols)
            {
                Console.WriteLine("{0, 20}\t:{1, 25}", entry.Key.Trim(), entry.Value);
            }
            Console.WriteLine("==== ==== ====");
        }

        public void Define(Symbol symbol)
        {
            Console.WriteLine($"Define symbol: {symbol}");
            if (_symbols.ContainsKey(symbol.Name))
            {
                throw new ArgumentException($"Symbol {symbol.Name} already defined");
            }

            _symbols.Add(symbol.Name, symbol);
        }

        public Symbol Lookup(string symbolName)
        {
            Console.WriteLine($"Lookup symbol: {symbolName} (scope {Name})");
            if (_symbols.ContainsKey(symbolName))
            {
                return _symbols[symbolName];
            }
            else if (EnclosingScope != null)
            {
                return EnclosingScope.Lookup(symbolName);
            }

            throw new NullReferenceException($"Symbol {symbolName} not found");
        }
    }
}
