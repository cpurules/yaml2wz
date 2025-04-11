using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace YamlToWz
{
    internal class YamlNpc : YamlImportObject
    {
        public static List<string> WZ_FILES = ["String.wz", "Npc.wz"];
        public string Path { get; set; }
        public int Id { get; set; }
        public int? CloneFrom { get; set; }

        public YamlNpcOverride Overrides { get; set; }

        public void AddToWz(Dictionary<string, WzFile> wzFiles)
        {
            var searchId = CloneFrom.HasValue ? CloneFrom.Value : Id;

            var wzString = wzFiles["String.wz"];
            var wzStringNpc = wzString.WzDirectory.GetImageByName("Npc.img").GetFromPath(searchId.ToString());
            var wzNpc = wzFiles["Npc.wz"];
            var wzNpcImg = wzNpc.WzDirectory.GetImageByName(searchId.ToString().PadLeft(7, '0') + ".img");
            if (wzNpcImg == null || wzStringNpc == null) throw new ArgumentOutOfRangeException("no npc with id " + searchId);

            if (CloneFrom.HasValue)
            {
                WzImageProperty clonedString = wzStringNpc.DeepClone();
                clonedString.Name = Id.ToString();
                wzString.WzDirectory.GetImageByName("Npc.img").AddAndUpdate(clonedString);
                wzStringNpc = clonedString;

                WzImage clonedNpc = wzNpcImg.DeepClone();
                clonedNpc.Name = Id.ToString().PadLeft(7, '0') + ".img";
                wzNpc.WzDirectory.AddImage(clonedNpc);
                wzNpcImg = clonedNpc;
            }

            if (Overrides != null)
            {
                // String.wz changes
                if (Overrides.Name != null) wzStringNpc.AddAndUpdate(new WzStringProperty("name", Overrides.Name));
                if (Overrides.Title != null) wzStringNpc.AddAndUpdate(new WzStringProperty("func", Overrides.Title));

                // Npc.wz changes
                if (Overrides.Script != null)
                {
                    var info = wzNpcImg.GetFromPath("info") ?? new WzSubProperty("info");
                    wzNpcImg.AddAndUpdate(info);
                    var script = new WzSubProperty("script");
                    info.AddAndUpdate(script);
                    var script0 = new WzSubProperty("0");
                    script.AddAndUpdate(script0);
                    script0.AddAndUpdate(new WzStringProperty("script", Overrides.Script));
                }
            }

        }
    }

    internal class YamlNpcOverride
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Script { get; set; }
    }
}
