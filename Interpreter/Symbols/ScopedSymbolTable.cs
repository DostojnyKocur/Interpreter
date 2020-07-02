﻿using System;
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
            Logger.DebugScope(Environment.NewLine);
            var enclosingScopeName = EnclosingScope != null ? EnclosingScope.Name : "none";
            Logger.DebugScope($"==== SYMBOL TABLE ({Name} : level {Level} : enclosing scope : {enclosingScopeName}) ====");
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
