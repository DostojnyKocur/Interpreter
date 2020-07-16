using Interpreter.AnalyzerService.Symbols;

namespace Interpreter.Symbols
{
    public class SymbolBuiltinFunction : SymbolFunction
    {
        public SymbolBuiltinFunction(string name, Symbol returnType)
            : base(name, returnType)
        {
        }
    }
}
