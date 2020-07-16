using System;
using System.Collections.Generic;
using System.Text;

namespace Interpreter.Memory
{
    public class ActivationRecord
    {
        public readonly Dictionary<string, object> _members = new Dictionary<string, object>();

        public ActivationRecord(string recordName, ActivationRecordType type, uint recordLevel) => (Name, Type, Level) = (recordName, type, recordLevel);

        public string Name { get; }
        public ActivationRecordType Type { get; }
        public uint Level { get; }

        public object this[string index]
        {
            get => _members[index];
            set
            {
                if(!_members.ContainsKey(index))
                {
                    _members.Add(index, null);
                }
                _members[index] = value;
            }
        }

        public object Get(string key)
        {
            return _members.ContainsKey(key) ? _members[key] : null;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder($"{Level}: {Type} {Name}");
            stringBuilder.Append(Environment.NewLine);
            foreach (var member in _members)
            {
                stringBuilder.Append(string.Format("{0,20}\t:\t{1,-30}", member.Key, GetMemberValueAsString(member.Value)));
                stringBuilder.Append(Environment.NewLine);
            }

            return stringBuilder.ToString();
        }

        private string GetMemberValueAsString(object value)
        {
            switch(value)
            {
                case IEnumerable<dynamic> enumerable:
                    var stringBuilder = new StringBuilder("[");
                    stringBuilder.Append(string.Join(", ", enumerable));
                    stringBuilder.Append("]");
                    return stringBuilder.ToString();
                default:
                    return value.ToString();
            }
        }
    }
}
