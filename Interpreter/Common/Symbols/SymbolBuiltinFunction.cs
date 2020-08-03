namespace Interpreter.Common.Symbols
{
    public class SymbolBuiltinFunction : SymbolFunction
    {
        public SymbolBuiltinFunction(string name, Symbol returnType)
            : base(name, returnType)
        {
        }
    }
}
