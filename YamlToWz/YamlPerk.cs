using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YamlToWz
{
    internal class YamlPerk
    {
        public string Path { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public YamlIcon Icon { get; set; }
        public string Description { get; set; }
        public string LongDescription { get; set; }
    }

    internal class YamlIcon
    {
        public String Path { get; set; }
    }
}
