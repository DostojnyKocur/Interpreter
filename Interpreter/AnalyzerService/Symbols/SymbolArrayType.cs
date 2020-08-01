namespace Interpreter.AnalyzerService.Symbols
{
    public class SymbolArrayType : Symbol
    {
        public SymbolArrayType(string name, Symbol baseType)
            : base(name, baseType) { }
    }
}
