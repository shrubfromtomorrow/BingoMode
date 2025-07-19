using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoMode.BingoRandomizer
{
    public struct Weighted<T>(T value, int weight)
    {
        public T value = value;
        public int weight = weight;
    }
}
