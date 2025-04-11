using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YamlToWz
{
    internal class YamlLife
    {
        public int Cy { get; set; }
        public int F { get; set; }
        public int Fh { get; set; }
        public string Id { get; set; }
        public int Rx0 { get; set; }
        public int Rx1 { get; set; }
        public YamlLifeType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int? MobTime { get; set; }
        public int? Hide { get; set; }
    }
    
    internal enum YamlLifeType
    {
        n,
    }
}
