using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YamlToWz
{
    internal class YamlMap : YamlImportObject
    {
        public static List<string> WZ_FILES = ["String.wz", "Map.wz"];
        public string Path { get; set; }
        public int Id { get; set; }

        public YamlMapOverride? Overrides { get; set; }

        public void AddToWz(Dictionary<string, WzFile> wzFiles)
        {
            if (Overrides != null)
            {
                // String.wz is so annoying for maps so we're breaking this up
                if (Overrides.Name != null || Overrides.Street != null)
                {
                    var wzString = wzFiles["String.wz"];
                    var wzStringMapImg = wzString.WzDirectory.GetImageByName("Map.img");
                    var wzStringMapImgProp =
                        wzStringMapImg.WzProperties.FirstOrDefault(
                            prop => wzStringMapImg.GetFromPath(prop.Name + "/" + Id.ToString()) != null, null);
                    if (wzStringMapImgProp == null)
                        throw new ArgumentOutOfRangeException("No entry in String.wz found for id " + Id);

                    var mapProp = wzStringMapImgProp.GetFromPath(Id.ToString());
                    if (Overrides.Name != null) mapProp.AddAndUpdate(new WzStringProperty("mapName", Overrides.Name));
                    if (Overrides.Street != null) mapProp.AddAndUpdate(new WzStringProperty("streetName", Overrides.Street));
                }

                var wzMap = wzFiles["Map.wz"];
                var wzMapImg = wzMap.WzDirectory.GetDirectoryByName("Map")
                    .GetDirectoryByName("Map" + Id.ToString().Substring(0, 1)).GetImageByName(Id.ToString() + ".img");
                if (wzMapImg == null) throw new ArgumentOutOfRangeException("no map found with id " + Id);
                bool addedOverride = false;
                var mapInfo = wzMapImg.GetFromPath("info") ?? new WzSubProperty("info");
                if (Overrides.EnterScript != null)
                {
                    addedOverride = true;
                    mapInfo.AddAndUpdate(new WzStringProperty("onUserEnter", Overrides.EnterScript));
                }

                if (Overrides.ReturnMap != null)
                {
                    addedOverride = true;
                    mapInfo.AddAndUpdate(new WzStringProperty("returnMap", Overrides.ReturnMap));
                }

                if (Overrides.ForcedReturnMap != null)
                {
                    addedOverride = true;
                    mapInfo.AddAndUpdate(new WzStringProperty("forcedReturn", Overrides.ForcedReturnMap));
                }

                if (addedOverride) wzMapImg.AddAndUpdate(mapInfo);
            }
        }
    }

    internal class YamlMapOverride
    {
        public string? Name { get; set; }
        public string? Street { get; set; }
        public string? EnterScript { get; set; }
        public string? ReturnMap { get; set; }
        public string? ForcedReturnMap { get; set; }
    }
}
