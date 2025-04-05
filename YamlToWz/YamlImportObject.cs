using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapleLib.WzLib;

namespace YamlToWz
{
    internal interface YamlImportObject
    {
        public static List<string> WZ_FILES { get; }
        public void AddToWz(Dictionary<string, WzFile> wzFiles);
    }
}
