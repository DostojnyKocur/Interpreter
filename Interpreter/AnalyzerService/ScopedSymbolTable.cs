using System;
using System.Collections.Generic;
using System.Linq;
using Interpreter.Common;
using Interpreter.Common.Symbols;

namespace Interpreter.AnalyzerService
{
    public class ScopedSymbolTable
    {
        private const string ArrayTypeSuffix = "_array";

        private readonly List<Symbol> _symbols = new List<Symbol>
        {
            new SymbolBuiltinType("void"),
            new SymbolBuiltinType("number"),
            new SymbolBuiltinType("bool"),
            new SymbolBuiltinType("string")
        };

        public ScopedSymbolTable(string scopeName, uint scopeLevel, ScopedSymbolTable enclosingScope = null)
        {
            Name = scopeName;
            Level = scopeLevel;
            EnclosingScope =  enclosingScope;

            InitializeArrayTypes();
            InitializeBuiltinFunctions();
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
                Logger.DebugScope(string.Format("{0, 20}\t:\t{1, -30}", entry.Name.Trim(), entry));
            }
            Logger.DebugScope("==== ==== ====");
        }

        public void Define(Symbol symbol)
        {
            Logger.DebugScope($"Define symbol: {symbol}");

            symbol.ScopeLevel = Level;
            _symbols.Add(symbol);
        }

        public Symbol LookupSingle(string symbolName, bool onlyCurrentScope = false)
        {
            Logger.DebugScope($"Lookup symbol: {symbolName} (scope {Name})");
            var result = _symbols.FirstOrDefault(item => item.Name == symbolName);

            if (result != null)
            {
                return result;
            }
            else if (EnclosingScope != null && !onlyCurrentScope)
            {
                return EnclosingScope.LookupSingle(symbolName);
            }

            return null;
        }

        public List<Symbol> LookupMany(string symbolName, bool onlyCurrentScope = false)
        {
            Logger.DebugScope($"Lookup symbol: {symbolName} (scope {Name})");
            var result = _symbols.Where(item => item.Name == symbolName).ToList();

            if (result.Any())
            {
                return result;
            }
            else if (EnclosingScope != null && !onlyCurrentScope)
            {
                return EnclosingScope.LookupMany(symbolName);
            }

            return new List<Symbol>();
        }

        private void InitializeBuiltinFunctions()
        {
            DefinePrintFunctionGroup("number");
            DefinePrintFunctionGroup("number_array");
            DefinePrintFunctionGroup("bool");
            DefinePrintFunctionGroup("bool_array");
            DefinePrintFunctionGroup("string");
            DefinePrintFunctionGroup("string_array");
        }

        private void DefinePrintFunctionGroup(string paramType)
        {
            var symbolVoid = LookupSingle("void");
            var symbolParamType = LookupSingle(paramType);

            var printFunction = new SymbolBuiltinFunction("print", symbolVoid);
            printFunction.Parameters.Add(new SymbolVariable("str", symbolParamType));
            _symbols.Add(printFunction);
        }

        private void InitializeArrayTypes()
        {
            DefineArrayType("number");
            DefineArrayType("bool");
            DefineArrayType("string");
        }

        private void DefineArrayType(string typeName)
        {
            var symbolParamType = LookupSingle(typeName);
            Define(new SymbolArrayType($"{typeName}{ArrayTypeSuffix}", symbolParamType));
        }
    }
}
