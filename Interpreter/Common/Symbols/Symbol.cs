using System;

namespace Interpreter.Common.Symbols
{
    public class Symbol
    {
        public Symbol(string name, Symbol type = null) => (Name, Type) = (name, type);

        public string Name { get; }
        public Symbol Type { get; }
        public uint ScopeLevel { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            var symbol = obj as Symbol;

            return Name == symbol.Name && Type == symbol.Type;
        }

        public static bool operator ==(Symbol left, Symbol right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Symbol left, Symbol right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type, ScopeLevel);
        }
    }
}
