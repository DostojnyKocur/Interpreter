using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Common.Extensions
{
    public static class CollectionExtensions
    {
        public static string ToPrint(this List<dynamic> collection)
        {
            var stringCollection = collection.Select(item => item.ToString());
            var joinedString = string.Join(", ", stringCollection);

            return $"[{joinedString}]";
        }
    }
}
