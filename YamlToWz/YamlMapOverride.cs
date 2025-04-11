using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using Microsoft.VisualBasic.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using String = System.String;

namespace YamlToWz
{
    internal class YamlMap : YamlImportObject
    {
        public static List<string> WZ_FILES = ["String.wz", "Map.wz"];
        public string Path { get; set; }
        public int Id { get; set; }
        public int? CloneFrom { get; set; }
        public List<YamlLife> AddLife { get; set; } = new();

        public YamlMapOverride? Overrides { get; set; }

        public void AddToWz(Dictionary<string, WzFile> wzFiles)
        {
            if (CloneFrom.HasValue)
            {
                var id = CloneFrom.Value;
                // String.wz...
                var wzString = wzFiles["String.wz"];
                var wzStringMapImg = wzString.WzDirectory.GetImageByName("Map.img");
                var wzStringMapImgProp =
                    wzStringMapImg.WzProperties.FirstOrDefault(
                        prop => wzStringMapImg.GetFromPath(prop.Name + "/" + id) != null, null);
                if (wzStringMapImgProp == null)
                    throw new ArgumentOutOfRangeException("No entry in String.wz found for clone id " + id);

                wzStringMapImgProp.GetFromPath(id.ToString()).DuplicateAs(Id.ToString());
                /*
                var stringClone = wzStringMapImgProp.GetFromPath(id.ToString()).Clone();
                if (stringClone == null) throw new Exception();
                wzStringMapImgProp.AddAndUpdate(stringClone);*/

                // Map.wz
                var wzMap = wzFiles["Map.wz"];
                var wzMapImg = wzMap.WzDirectory.GetDirectoryByName("Map")
                    .GetDirectoryByName("Map" + id.ToString().Substring(0, 1)).GetImageByName(id + ".img");
                if (wzMapImg == null) throw new ArgumentOutOfRangeException("no map found with id " + Id);
                wzMapImg.DuplicateAs(Id + ".img");
            }

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

            if (AddLife.Count > 0)
            {
                var wzMap = wzFiles["Map.wz"];
                var wzMapImg = wzMap.WzDirectory.GetDirectoryByName("Map")
                    .GetDirectoryByName("Map" + Id.ToString().Substring(0, 1)).GetImageByName(Id.ToString() + ".img");
                if (wzMapImg == null) throw new ArgumentOutOfRangeException("no map found with id " + Id);

                var lifeInfo = wzMapImg.GetFromPath("life") ?? new WzSubProperty("life");
                var maxLife = lifeInfo.WzProperties.Select(x => x.Name).Where(x => int.TryParse(x, out int z) && z >= 0)
                    .Select(x => int.Parse(x)).DefaultIfEmpty(-1).Max();

                foreach (var addLife in AddLife)
                {
                    var lifeNode = new WzSubProperty((++maxLife).ToString());
                    lifeInfo.AddAndUpdate(lifeNode);
                    lifeNode.AddAndUpdate(new WzStringProperty("id", addLife.Id));
                    lifeNode.AddAndUpdate(new WzStringProperty("type", addLife.Type.ToString()));
                    lifeNode.AddAndUpdate(new WzIntProperty("x", addLife.X));
                    lifeNode.AddAndUpdate(new WzIntProperty("y", addLife.Y));
                    lifeNode.AddAndUpdate(new WzIntProperty("rx0", addLife.Rx0));
                    lifeNode.AddAndUpdate(new WzIntProperty("rx1", addLife.Rx1));
                    lifeNode.AddAndUpdate(new WzIntProperty("cy", addLife.Cy));
                    lifeNode.AddAndUpdate(new WzIntProperty("f", addLife.F));
                    lifeNode.AddAndUpdate(new WzIntProperty("fh", addLife.Fh));
                    if (addLife.MobTime.HasValue) lifeNode.AddAndUpdate(new WzIntProperty("mobTime", addLife.MobTime.Value));
                    if (addLife.Hide.HasValue) lifeNode.AddAndUpdate(new WzIntProperty("hide", addLife.Hide.Value));
                }
                wzMapImg.AddAndUpdate(lifeInfo);
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
