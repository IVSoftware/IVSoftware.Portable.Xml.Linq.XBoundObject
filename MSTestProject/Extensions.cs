using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBoundObjectMSTest
{
    static partial class TestExtensions
    {
        public static T DequeueSingle<T>(this Queue<T> @this)
        {
            T eventPair = @this.Dequeue();
            if (@this.Count > 0)
            {
                throw new InvalidOperationException($"{typeof(T).Name} objects remain after single dequeue");
            }
            return eventPair;
        }
    }
}
