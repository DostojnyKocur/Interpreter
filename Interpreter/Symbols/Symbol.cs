namespace Interpreter.Symbols
{
    public class Symbol
    {
        public Symbol(string name, Symbol type = null) => (Name, Type) = (name, type);

        public string Name { get; }
        public Symbol Type { get; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
