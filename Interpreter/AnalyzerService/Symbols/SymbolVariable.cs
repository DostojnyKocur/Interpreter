using Interpreter.AnalyzerService.Symbols;

namespace Interpreter.Symbols
{
    public class SymbolVariable : Symbol
    {
        public SymbolVariable(string name, Symbol type)
            : base(name, type) { }

        public override string ToString()
        {
            return $"<{Name}:{Type}>";
        }
    }
}
