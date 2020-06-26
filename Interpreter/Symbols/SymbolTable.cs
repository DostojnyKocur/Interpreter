using System;
using System.Collections.Generic;

namespace Interpreter.Symbols
{
    public class SymbolTable
    {
        private readonly Dictionary<string, Symbol> _symbols = new Dictionary<string, Symbol>
        {
            { "number", new Symbol("number") }
        };

        public void DebugPrintSymbols()
        {
            Console.WriteLine("==== SYMBOL TABLE ====");
            foreach (var entry in _symbols)
            {
                Console.WriteLine($"{entry.Key}\t:{entry.Value}");
            }
            Console.WriteLine("==== ==== ====");
        }

        public void Define(Symbol symbol)
        {
            Console.WriteLine($"Define symbol: {symbol}");
            _symbols.Add(symbol.Name, symbol);
        }

        public Symbol Lookup(string symbolName)
        {
            Console.WriteLine($"Lookup symbol: {symbolName}");
            if(_symbols.ContainsKey(symbolName))
            {
                return _symbols[symbolName];
            }

            throw new NullReferenceException($"Symbol {symbolName} not found");
        }
    }
}
